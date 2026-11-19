using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;
using RentalLawAssistant.Evals;

// Evals console: runs evals/questions.json and writes evals/results/latest.md.
// Usage: dotnet run --project tools/RentalLawAssistant.Evals -- --retrieval-only
const string QuestionsPath = "evals/questions.json";
const string ReportPath = "evals/results/latest.md";

var retrievalOnly = args.Contains("--retrieval-only", StringComparer.Ordinal);
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = [.. args.Where(a => a != "--retrieval-only")],
    // Read appsettings.json from the build output, so the app works from any working directory.
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services.AddRentalLawAssistantOptions(builder.Configuration);
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(new Uri(ollama.Endpoint), ollama.EmbeddingModel);
});

using var host = builder.Build();
await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
try
{
    if (!retrievalOnly)
    {
        logger.LogError("Only --retrieval-only is available for now; the full mode arrives with answer generation (phase 2).");
        return 1;
    }

    var ollama = host.Services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    var corpus = host.Services.GetRequiredService<IOptions<CorpusOptions>>().Value;
    var retrievalOptions = host.Services.GetRequiredService<IOptions<RetrievalOptions>>();
    var store = await InMemoryArticleStore.LoadAsync(RepositoryPaths.Resolve(corpus.IndexPath), ollama.EmbeddingModel);
    var retriever = new Retriever(host.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(), store, retrievalOptions);

    var questions = EvalQuestion.Load(RepositoryPaths.Resolve(QuestionsPath));
    logger.LogInformation("Running {Count} questions against {Fragments} indexed fragments ({Model})", questions.Count, store.Count, store.EmbeddingModel);

    var results = new List<QuestionResult>();
    foreach (var question in questions)
    {
        var stopwatch = Stopwatch.StartNew();
        var retrieval = await retriever.RetrieveAsync(question.Question);
        results.Add(new QuestionResult(question, retrieval, stopwatch.Elapsed));
    }

    var report = MarkdownReport.Build(
        results, store.EmbeddingModel, retrievalOptions.Value.TopK, retrievalOptions.Value.MinSimilarity, DateTimeOffset.UtcNow);
    var reportPath = RepositoryPaths.Resolve(ReportPath);
    Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
    await File.WriteAllTextAsync(reportPath, report);

    Console.WriteLine();
    Console.WriteLine(report);
    logger.LogInformation("Report written to {ReportPath}", reportPath);
    return 0;
}
catch (Exception exception) when (exception is FileNotFoundException or InvalidDataException or InvalidOperationException)
{
    logger.LogError("{Message}", exception.Message);
    return 1;
}
catch (HttpRequestException exception)
{
    logger.LogError("Could not reach Ollama ({Message}). Is it running? Check with: ollama list", exception.Message);
    return 2;
}
finally
{
    await host.StopAsync();
}
