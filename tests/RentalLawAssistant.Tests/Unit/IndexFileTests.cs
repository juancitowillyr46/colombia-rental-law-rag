using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;

namespace RentalLawAssistant.Tests.Unit;

public sealed class IndexFileTests
{
    [Fact]
    public async Task RoundTripsTheIndex()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}", "articles.index.json");
        var article = new LawArticle("lp-1", "lp", "Ley de Prueba", "1", "Texto.", InForce: true, "https://example.org");
        var index = new ArticleIndex("bge-m3", 2, new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero), [new IndexedArticle(article, "abc", "bge-m3", [0.5f, -0.25f])]);
        try
        {
            await IndexFile.WriteAsync(path, index);
            var read = await IndexFile.ReadAsync(path);

            Assert.Equal(index.EmbeddingModel, read.EmbeddingModel);
            Assert.Equal(index.CreatedAt, read.CreatedAt);
            var single = Assert.Single(read.Articles);
            Assert.Equal(article, single.Article);
            Assert.Equal([0.5f, -0.25f], single.Embedding);
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public async Task MissingFileTellsToRunTheIngestion()
    {
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => IndexFile.ReadAsync(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json")));

        Assert.Contains("Run the ingestion", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RelativePathsResolveFromTheRepositoryRoot()
    {
        var resolved = RepositoryPaths.Resolve("data/corpus");

        Assert.True(File.Exists(Path.Combine(resolved, "..", "..", RepositoryPaths.MarkerFileName)));
        Assert.EndsWith(Path.Combine("data", "corpus"), resolved, StringComparison.Ordinal);
    }

    [Fact]
    public void AbsolutePathsAreKept()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "corpus");

        Assert.Equal(Path.GetFullPath(absolute), RepositoryPaths.Resolve(absolute));
    }
}
