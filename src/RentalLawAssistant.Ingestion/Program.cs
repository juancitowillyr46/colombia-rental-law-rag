using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;

// Ingestion console: parses data/corpus, embeds new or changed fragments and writes the index file.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // Read appsettings.json from the build output, so the app works from any working directory.
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services.AddRentalLawAssistantOptions(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(_ => new CorpusParser());
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(ollama.CreateHttpClient(), ollama.EmbeddingModel);
});
builder.Services.AddSingleton<Ingestor>();

using var host = builder.Build();

// StartAsync runs the ValidateOnStart checks: an invalid configuration stops here with a clear error.
await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<OllamaOptions>>().Value;
try
{
    logger.LogInformation("Ingesting corpus with embedding model {Model} at {Endpoint}", options.EmbeddingModel, options.Endpoint);
    var summary = await host.Services.GetRequiredService<Ingestor>().RunAsync();

    if (summary.PreviousIndexWarning is not null)
    {
        logger.LogWarning("{Warning}", summary.PreviousIndexWarning);
    }

    logger.LogInformation(
        "Index written to {IndexPath}\n" +
        "  Articles: {Articles} ({InForce} in force, {Repealed} repealed) in {Fragments} fragments\n" +
        "  Embedded: {Embedded}, reused: {Reused}, dimensions: {Dimensions}\n" +
        "  Time: {Seconds:F1} s",
        summary.IndexPath,
        summary.Articles,
        summary.InForce,
        summary.Repealed,
        summary.Fragments,
        summary.Embedded,
        summary.Reused,
        summary.Dimensions,
        summary.Elapsed.TotalSeconds);
    return 0;
}
catch (CorpusFormatException exception)
{
    logger.LogError("Invalid corpus, nothing was written: {Message}", exception.Message);
    return 1;
}
catch (HttpRequestException exception)
{
    logger.LogError(
        "Could not use Ollama at {Endpoint} ({Message}). Is it running, and is '{Model}' pulled? Check with: ollama list",
        options.Endpoint,
        exception.Message,
        options.EmbeddingModel);
    return 2;
}
finally
{
    await host.StopAsync();
}
