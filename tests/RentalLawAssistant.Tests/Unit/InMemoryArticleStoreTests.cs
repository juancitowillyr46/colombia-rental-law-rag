using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;

namespace RentalLawAssistant.Tests.Unit;

public sealed class InMemoryArticleStoreTests
{
    // 2-D vectors make the expected order obvious: similarity to (1, 0) is the cosine of the angle.
    private static readonly ArticleIndex Index = new("bge-m3", 2, DateTimeOffset.UnixEpoch,
    [
        Fragment("t-1", "1", [1f, 0f]),                  // same direction as the query: 1.0
        Fragment("t-2", "2", [1f, 1f]),                  // 45 degrees: ~0.707
        Fragment("t-3", "3", [0f, 1f]),                  // orthogonal: 0.0
        Fragment("t-4", "4", [0.9f, 0.1f], inForce: false), // very close, but repealed
        Fragment("t-5#1", "5", [-1f, 0f], content: "Primer inciso."),
        Fragment("t-5#2", "5", [-1f, 0.1f], content: "Segundo inciso."),
    ]);

    private static readonly float[] QueryAlongX = [1f, 0f];
    private static readonly float[] ThreeDimensionalQuery = [1f, 0f, 0f];

    private readonly InMemoryArticleStore _store = new(Index);

    [Fact]
    public async Task OrdersByDescendingSimilarity()
    {
        var results = await _store.SearchAsync(QueryAlongX, topK: 3, inForceOnly: true, CancellationToken.None);

        Assert.Equal(["t-1", "t-2", "t-3"], results.Select(r => r.Article.Id));
        Assert.Equal(1f, results[0].Similarity, precision: 4);
        Assert.Equal(MathF.Sqrt(0.5f), results[1].Similarity, precision: 4);
        Assert.Equal(0f, results[2].Similarity, precision: 4);
    }

    [Fact]
    public async Task FiltersRepealedArticlesWhenAskedTo()
    {
        var inForce = await _store.SearchAsync(QueryAlongX, topK: 10, inForceOnly: true, CancellationToken.None);
        var all = await _store.SearchAsync(QueryAlongX, topK: 10, inForceOnly: false, CancellationToken.None);

        Assert.DoesNotContain(inForce, r => r.Article.Id == "t-4");
        Assert.Equal("t-4", all[1].Article.Id);
    }

    [Fact]
    public async Task ReturnsAtMostTopK()
    {
        var results = await _store.SearchAsync(QueryAlongX, topK: 2, inForceOnly: false, CancellationToken.None);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task RejectsQueriesWithTheWrongDimensions()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _store.SearchAsync(ThreeDimensionalQuery, topK: 1, inForceOnly: true, CancellationToken.None));
    }

    [Fact]
    public async Task GetsFragmentsAndWholeArticlesById()
    {
        var fragment = await _store.GetByIdAsync("t-5#2", CancellationToken.None);
        var article = await _store.GetByIdAsync("t-5", CancellationToken.None);
        var missing = await _store.GetByIdAsync("t-99", CancellationToken.None);

        Assert.Equal("Segundo inciso.", fragment?.Content);
        Assert.Equal("t-5", article?.Id);
        Assert.Equal("Primer inciso.\n\nSegundo inciso.", article?.Content);
        Assert.Null(missing);
    }

    [Fact]
    public async Task LoadRefusesAnIndexBuiltWithAnotherModel()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        try
        {
            await IndexFile.WriteAsync(path, Index);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => InMemoryArticleStore.LoadAsync(path, expectedEmbeddingModel: "nomic-embed-text"));

            Assert.Contains("'bge-m3'", exception.Message, StringComparison.Ordinal);
            Assert.Contains("Run the ingestion again", exception.Message, StringComparison.Ordinal);
            Assert.Equal(6, (await InMemoryArticleStore.LoadAsync(path, "bge-m3")).Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static IndexedArticle Fragment(string id, string number, float[] embedding, bool inForce = true, string? content = null) =>
        new(new LawArticle(id, "t", "Ley de Prueba", number, content ?? $"Texto {id}.", inForce, "https://example.org"), "hash", "bge-m3", embedding);
}
