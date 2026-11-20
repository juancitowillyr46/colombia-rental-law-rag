using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;
using RentalLawAssistant.Api;
using RentalLawAssistant.Core;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRentalLawAssistantOptions(builder.Configuration);
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(ollama.CreateHttpClient(), ollama.EmbeddingModel);
});
builder.Services.AddSingleton<IChatClient>(services =>
{
    var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    return new OllamaApiClient(ollama.CreateHttpClient(), ollama.ChatModel);
});
builder.Services.AddRentalLawAssistant();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Colombia Rental Law RAG";
    document.Info.Version = "v1";
    document.Info.Description =
        "Answers Colombian residential rental law questions using only official legal texts, always citing the article. " +
        "Runs locally with Ollama. General information, not legal advice.";
    return Task.CompletedTask;
}));

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

if (app.Environment.IsDevelopment())
{
    // OpenAPI document at /openapi/v1.json and interactive docs at /scalar.
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Colombia Rental Law RAG")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.MapAskEndpoints();

app.Run();

// Exposes the entry point to WebApplicationFactory in integration tests.
public partial class Program;
