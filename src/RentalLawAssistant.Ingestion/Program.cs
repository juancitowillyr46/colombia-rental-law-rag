using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Options;

// Ingestion console: parses data/corpus, embeds articles with bge-m3 and writes the index file (phase 1, step 6).
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // Read appsettings.json from the build output, so the app works from any working directory.
    ContentRootPath = AppContext.BaseDirectory,
});
builder.Services.AddRentalLawAssistantOptions(builder.Configuration);

using var host = builder.Build();

// StartAsync runs the ValidateOnStart checks: an invalid configuration stops here with a clear error.
await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var ollama = host.Services.GetRequiredService<IOptions<OllamaOptions>>().Value;
logger.LogInformation("Ingestion configured: embedding model {EmbeddingModel} at {Endpoint}", ollama.EmbeddingModel, ollama.Endpoint);
logger.LogWarning("Ingestion is not implemented yet.");

await host.StopAsync();
