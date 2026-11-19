using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RentalLawAssistant.Api;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Messages;
using RentalLawAssistant.Tests.Fakes;

namespace RentalLawAssistant.Tests.Integration;

/// <summary>The real API pipeline with a test index and fake models: no Ollama needed.</summary>
public sealed class AskEndpointsTests : IAsyncLifetime
{
    private readonly string _indexPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.index.json");

    public Task InitializeAsync() => IndexFile.WriteAsync(_indexPath, TestCorpus.Index);

    public Task DisposeAsync()
    {
        File.Delete(_indexPath);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AnswersWithCitationsFromTheCorpus()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("Lo paga el propietario.", grounded: true, "cc-1985", "cc-7777"));
        using var factory = CreateFactory(chat);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/ask", new { question = TestCorpus.GoodQuestion });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal("Lo paga el propietario.", root.GetProperty("answer").GetString());
        Assert.True(root.GetProperty("grounded").GetBoolean());
        var citation = Assert.Single(root.GetProperty("citations").EnumerateArray());
        Assert.Equal("cc-1985", citation.GetProperty("id").GetString());
        Assert.Equal("Código Civil", citation.GetProperty("law").GetString());
        Assert.Equal("1985", citation.GetProperty("article").GetString());
        Assert.Equal("Texto del 1985 sobre reparaciones necesarias.", citation.GetProperty("text").GetString());
        Assert.Equal("https://example.org/cc", citation.GetProperty("sourceUrl").GetString());
        Assert.Equal(SpanishMessages.Disclaimer, root.GetProperty("disclaimer").GetString());
        Assert.Equal("2026-11-19", root.GetProperty("corpusCutoff").GetString());
    }

    [Fact]
    public async Task DeclinesWithoutCallingTheModelWhenThereIsNoBasis()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("No debería llamarse.", grounded: true, "cc-1985"));
        using var factory = CreateFactory(chat);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/ask", new { question = TestCorpus.UnrelatedQuestion });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("grounded").GetBoolean());
        Assert.Equal(SpanishMessages.NoBasis, json.RootElement.GetProperty("answer").GetString());
        Assert.Empty(json.RootElement.GetProperty("citations").EnumerateArray());
        Assert.Equal(0, chat.Calls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task RejectsEmptyQuestions(string? question)
    {
        using var factory = CreateFactory(new FakeChatClient("unused"));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/ask", new { question });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(SpanishMessages.QuestionRequired, await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task MalformedJsonIsABadRequestNotAServerError(string environment)
    {
        using var factory = CreateFactory(new FakeChatClient("unused"), environment);
        using var client = factory.CreateClient();
        using var body = new StringContent("{ \"question\": ", System.Text.Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/ask", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RejectsTooLongQuestions()
    {
        using var factory = CreateFactory(new FakeChatClient("unused"));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/ask", new { question = new string('a', AskEndpoints.MaxQuestionLength + 1) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("demasiado larga", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DebugEndpointShowsRetrievalInDevelopment()
    {
        using var factory = CreateFactory(new FakeChatClient("unused"), environment: "Development");
        using var client = factory.CreateClient();

        using var json = JsonDocument.Parse(await client.GetStringAsync($"/ask/debug?question={Uri.EscapeDataString(TestCorpus.GoodQuestion)}"));

        var articles = json.RootElement.GetProperty("articles").EnumerateArray().ToList();
        Assert.Equal("cc-1985", articles[0].GetProperty("id").GetString());
        Assert.True(json.RootElement.GetProperty("hasBasis").GetBoolean());
        Assert.DoesNotContain(articles, a => a.GetProperty("articleId").GetString() == "cc-2035");
    }

    [Fact]
    public async Task DebugEndpointDoesNotExistOutsideDevelopment()
    {
        using var factory = CreateFactory(new FakeChatClient("unused"), environment: "Production");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/ask/debug?question=hola");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiDoesNotStartWhenTheIndexWasBuiltWithAnotherModel()
    {
        await IndexFile.WriteAsync(_indexPath, TestCorpus.Index with { EmbeddingModel = "nomic-embed-text" });
        using var factory = CreateFactory(new FakeChatClient("unused"));

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        Assert.Contains("'nomic-embed-text'", MessagesOf(exception), StringComparison.Ordinal);
    }

    private WebApplicationFactory<Program> CreateFactory(FakeChatClient chat, string environment = "Development") =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("Corpus:IndexPath", _indexPath);
            builder.UseSetting("Corpus:CutoffDate", "2026-11-19");
            builder.UseSetting("Retrieval:MinSimilarity", "0.5");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmbeddingGenerator<string, Embedding<float>>>();
                services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(TestCorpus.Embeddings());
                services.RemoveAll<IChatClient>();
                services.AddSingleton<IChatClient>(chat);
            });
        });

    private static string MessagesOf(Exception exception)
    {
        var messages = new List<string>();
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }

        return string.Join(" | ", messages);
    }
}
