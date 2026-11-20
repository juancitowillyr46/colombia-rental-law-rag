using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using RentalLawAssistant.Core;
using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;
using RentalLawAssistant.Evals;

// Evals console: runs evals/questions.json and writes a Markdown report.
//   dotnet run --project tools/RentalLawAssistant.Evals -- --retrieval-only    retrieval metrics, no chat model
//   dotnet run --project tools/RentalLawAssistant.Evals                        full pipeline (retrieval + answer + validation)
//   add --final to also run the held-out test split (only once, at the end)
//   add --Ollama:ChatModel=<model> or --Retrieval:MinSimilarity=<value> to compare settings
const string QuestionsPath = "evals/questions.json";
const string RetrievalOnlyFlag = "--retrieval-only";
const string FinalFlag = "--final";

var retrievalOnly = args.Contains(RetrievalOnlyFlag, StringComparer.Ordinal);
var includeTest = args.Contains(FinalFlag, StringComparer.Ordinal);
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = [.. args.Where(a => a is not (RetrievalOnlyFlag or FinalFlag))],
    // Read appsettings.json from the build output, so the app works from any working directory.
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services.AddRentalLawAssistantOptions(builder.Configuration);
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(new Uri(ollama.Endpoint), ollama.EmbeddingModel);
});
builder.Services.AddSingleton<IChatClient>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(new Uri(ollama.Endpoint), ollama.ChatModel);
});
builder.Services.AddRentalLawAssistant();

using var host = builder.Build();
await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
try
{
    var ollama = host.Services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    var retrievalOptions = host.Services.GetRequiredService<IOptions<RetrievalOptions>>().Value;
    var retriever = host.Services.GetRequiredService<Retriever>();
    var askService = host.Services.GetRequiredService<AskService>();

    // The test split is held out: it only runs with --final, once, after tuning is done.
    var questions = EvalQuestion.Load(RepositoryPaths.Resolve(QuestionsPath))
        .Where(q => includeTest || q.Split == EvalQuestion.TuningSplit)
        .ToList();
    logger.LogInformation(
        "Running {Count} questions ({Mode}{Final}) with embedding model {EmbeddingModel}{ChatModel}",
        questions.Count,
        retrievalOnly ? "retrieval only" : "full",
        includeTest ? ", including the test split" : ", tuning split only",
        ollama.EmbeddingModel,
        retrievalOnly ? string.Empty : $" and chat model {ollama.ChatModel}");

    var results = new List<QuestionResult>();
    foreach (var question in questions)
    {
        var retrieval = await retriever.RetrieveAsync(question.Question);
        var stopwatch = Stopwatch.StartNew();
        var ask = retrievalOnly ? null : await askService.AskAsync(question.Question);
        var latency = retrievalOnly ? TimeSpan.Zero : stopwatch.Elapsed;
        if (retrievalOnly)
        {
            // Retrieval-only latency: time a second retrieval so the model warm-up does not dominate.
            stopwatch.Restart();
            await retriever.RetrieveAsync(question.Question);
            latency = stopwatch.Elapsed;
        }

        results.Add(new QuestionResult(question, retrieval, latency, ask));
        logger.LogInformation("{Id}: {Outcome} ({Seconds:F1} s)", question.Id, results[^1].Failed ? "FAIL" : "ok", latency.TotalSeconds);
    }

    var settings = new ReportSettings(
        ollama.EmbeddingModel,
        retrievalOnly ? null : ollama.ChatModel,
        retrievalOptions.TopK,
        retrievalOptions.MinSimilarity,
        includeTest,
        DateTimeOffset.UtcNow);
    var report = MarkdownReport.Build(results, settings);
    var reportPath = RepositoryPaths.Resolve(ReportFileName(settings));
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

// latest.md is the retrieval-only tuning run; full and final runs get their own files so they can be compared.
static string ReportFileName(ReportSettings settings)
{
    var model = settings.ChatModel is null ? null : string.Concat(settings.ChatModel.Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' ? c : '-'));
    var name = (settings.ChatModel, settings.IncludeTest) switch
    {
        (null, false) => "latest",
        (null, true) => "final-retrieval",
        (_, false) => $"full-{model}",
        (_, true) => $"final-{model}",
    };
    return $"evals/results/{name}.md";
}
