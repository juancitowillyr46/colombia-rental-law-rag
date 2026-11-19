using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;
using RentalLawAssistant.Api;
using RentalLawAssistant.Core;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddProblemDetails();

var app = builder.Build();

// Load the index now: a missing index or an embedding-model mismatch must stop the API at startup,
// not fail on the first request.
app.Services.GetRequiredService<IArticleStore>();

// Malformed request bodies (BadHttpRequestException) are client errors: 400, not 500.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});
app.MapAskEndpoints();

app.Run();

// Exposes the entry point to WebApplicationFactory in integration tests.
public partial class Program;
