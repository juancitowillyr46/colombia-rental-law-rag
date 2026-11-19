using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Messages;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;
using RentalLawAssistant.Tests.Fakes;

namespace RentalLawAssistant.Tests.Unit;

public sealed class AskServiceTests
{
    private static readonly InMemoryArticleStore Store = new(TestCorpus.Index);

    [Fact]
    public async Task AnswersWithCitationsWhoseTextComesFromTheCorpus()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("Lo paga el propietario.", grounded: true, "cc-1985"));

        var result = await CreateService(chat).AskAsync(TestCorpus.GoodQuestion);

        Assert.True(result.Grounded);
        Assert.Equal("Lo paga el propietario.", result.Answer);
        var citation = Assert.Single(result.Citations);
        Assert.Equal(("cc-1985", "Código Civil", "1985"), (citation.Id, citation.Law, citation.Article));
        Assert.Equal("Texto del 1985 sobre reparaciones necesarias.", citation.Text);
        Assert.Equal(SpanishMessages.Disclaimer, result.Disclaimer);
        Assert.Equal(new DateOnly(2026, 11, 19), result.CorpusCutoff);
    }

    [Fact]
    public async Task InventedCitationNeverReachesTheAnswer()
    {
        // The model cites one real retrieved article and two it made up.
        var chat = new FakeChatClient(FakeChatClient.Answer("Respuesta.", grounded: true, "cc-2028", "cc-1234", "l820-99"));

        var result = await CreateService(chat).AskAsync(TestCorpus.GoodQuestion);

        Assert.Equal(["cc-2028"], result.Citations.Select(c => c.Id));
        Assert.Equal(2, result.DiscardedCitations);
    }

    [Fact]
    public async Task OnlyInventedCitationsMeansNoBasis()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("Respuesta inventada.", grounded: true, "cc-1234"));

        var result = await CreateService(chat).AskAsync(TestCorpus.GoodQuestion);

        AssertNoBasis(result);
        Assert.Equal(1, result.DiscardedCitations);
    }

    [Fact]
    public async Task BelowThresholdAnswersNoBasisWithoutCallingTheChatModel()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("No debería llamarse.", grounded: true, "cc-1985"));

        var result = await CreateService(chat).AskAsync(TestCorpus.UnrelatedQuestion);

        AssertNoBasis(result);
        Assert.Equal(0, chat.Calls);
    }

    [Fact]
    public async Task ModelSayingNotGroundedMeansNoBasis()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("Los artículos no responden.", grounded: false));

        AssertNoBasis(await CreateService(chat).AskAsync(TestCorpus.GoodQuestion));
    }

    [Fact]
    public async Task InvalidJsonFromTheModelMeansNoBasis()
    {
        var chat = new FakeChatClient("Claro, el propietario paga. {no es json");

        AssertNoBasis(await CreateService(chat).AskAsync(TestCorpus.GoodQuestion));
    }

    [Fact]
    public async Task ModelFailureMeansNoBasisInsteadOfAnError()
    {
        var chat = new FakeChatClient("unused") { Failure = new HttpRequestException("Connection refused") };

        AssertNoBasis(await CreateService(chat).AskAsync(TestCorpus.GoodQuestion));
    }

    [Fact]
    public async Task RepealedArticlesNeverAppearInCitations()
    {
        // cc-2035 is repealed: it is never retrieved, so citing it is discarded like an invention.
        var chat = new FakeChatClient(FakeChatClient.Answer("Respuesta.", grounded: true, "cc-2035", "cc-1985"));

        var result = await CreateService(chat).AskAsync(TestCorpus.GoodQuestion);

        Assert.DoesNotContain(result.Citations, c => c.Id == "cc-2035");
        Assert.DoesNotContain("cc-2035", chat.LastMessages[^1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ChunkedArticleIsCitedWithItsFullText()
    {
        var generator = TestCorpus.Embeddings();
        generator.SetVector("¿Servicios públicos?", [0.1f, 1f]);
        var chat = new FakeChatClient(FakeChatClient.Answer("Respuesta.", grounded: true, "l820-15"));

        var result = await CreateService(chat, generator).AskAsync("¿Servicios públicos?");

        var citation = Assert.Single(result.Citations);
        Assert.Equal("l820-15", citation.Id);
        Assert.Equal("Primer inciso del 15.\n\nSegundo inciso del 15.", citation.Text);
    }

    [Fact]
    public async Task SendsTheRetrievedArticleIdsToTheModelWithLowTemperature()
    {
        var chat = new FakeChatClient(FakeChatClient.Answer("Respuesta.", grounded: true, "cc-1985"));

        await CreateService(chat).AskAsync(TestCorpus.GoodQuestion);

        Assert.Equal(ChatRole.System, chat.LastMessages[0].Role);
        Assert.Contains("[cc-1985] Código Civil, artículo 1985", chat.LastMessages[1].Text, StringComparison.Ordinal);
        Assert.Contains(TestCorpus.GoodQuestion, chat.LastMessages[1].Text, StringComparison.Ordinal);
        Assert.Equal(AnswerGenerator.Temperature, chat.LastOptions?.Temperature);
    }

    private static void AssertNoBasis(AskResult result)
    {
        Assert.False(result.Grounded);
        Assert.Equal(SpanishMessages.NoBasis, result.Answer);
        Assert.Empty(result.Citations);
        Assert.Equal(SpanishMessages.Disclaimer, result.Disclaimer);
    }

    private static AskService CreateService(FakeChatClient chat, FakeEmbeddingGenerator? generator = null)
    {
        var retriever = new Retriever(
            generator ?? TestCorpus.Embeddings(), Store, Options.Create(new RetrievalOptions { TopK = 5, MinSimilarity = 0.5f }));
        var answerGenerator = new AnswerGenerator(chat, NullLogger<AnswerGenerator>.Instance);
        var corpus = Options.Create(new CorpusOptions { Directory = "x", IndexPath = "x", CutoffDate = new DateOnly(2026, 11, 19) });
        return new AskService(retriever, answerGenerator, Store, corpus, NullLogger<AskService>.Instance);
    }
}
