using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;
using RentalLawAssistant.Tests.Fakes;

namespace RentalLawAssistant.Tests.Unit;

public sealed class RetrieverTests
{
    private const string Question = "¿Quién paga el calentador?";

    private static readonly InMemoryArticleStore Store = new(new ArticleIndex("bge-m3", 2, DateTimeOffset.UnixEpoch,
    [
        Fragment("t-1#1", "1", [1f, 0f]),
        Fragment("t-1#2", "1", [0.95f, 0.05f]),
        Fragment("t-2", "2", [0.8f, 0.6f]),
        Fragment("t-3", "3", [0f, 1f]),
        Fragment("t-4", "4", [1f, 0.01f], inForce: false),
    ]));

    [Fact]
    public async Task EmbedsTheQuestionAsIs()
    {
        var generator = QuestionPointingTo(1f, 0f);

        await CreateRetriever(generator).RetrieveAsync(Question);

        Assert.Equal([Question], generator.Inputs);
    }

    [Fact]
    public async Task ReturnsDistinctInForceArticlesBestFirst()
    {
        var result = await CreateRetriever(QuestionPointingTo(1f, 0f), topK: 3).RetrieveAsync(Question);

        Assert.Equal(["t-1#1", "t-2", "t-3"], result.Articles.Select(a => a.Article.Id));
        Assert.DoesNotContain(result.Articles, a => !a.Article.InForce);
    }

    [Fact]
    public async Task HasBasisWhenTheBestMatchClearsTheThreshold()
    {
        var result = await CreateRetriever(QuestionPointingTo(1f, 0f), minSimilarity: 0.9f).RetrieveAsync(Question);

        Assert.True(result.HasBasis);
        Assert.Equal(1f, result.TopSimilarity, precision: 4);
    }

    [Fact]
    public async Task NoBasisBelowTheThresholdButStillReturnsCandidates()
    {
        // (-1, -1) points away from every stored vector: the best similarity is negative.
        var result = await CreateRetriever(QuestionPointingTo(-1f, -1f), minSimilarity: 0.5f).RetrieveAsync(Question);

        Assert.False(result.HasBasis);
        Assert.True(result.TopSimilarity < 0.5f);
        Assert.NotEmpty(result.Articles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RejectsEmptyQuestions(string question)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateRetriever(QuestionPointingTo(1f, 0f)).RetrieveAsync(question));
    }

    private static FakeEmbeddingGenerator QuestionPointingTo(float x, float y)
    {
        var generator = new FakeEmbeddingGenerator(dimensions: 2);
        generator.SetVector(Question, [x, y]);
        return generator;
    }

    private static Retriever CreateRetriever(FakeEmbeddingGenerator generator, int topK = 5, float minSimilarity = 0.5f) =>
        new(generator, Store, Options.Create(new RetrievalOptions { TopK = topK, MinSimilarity = minSimilarity }));

    private static IndexedArticle Fragment(string id, string number, float[] embedding, bool inForce = true) =>
        new(new LawArticle(id, "t", "Ley de Prueba", number, $"Texto {id}.", inForce, "https://example.org"), "hash", "bge-m3", embedding);
}
