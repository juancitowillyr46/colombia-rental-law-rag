using System.Text.Json;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Tests.Fakes;

namespace RentalLawAssistant.Tests.Unit;

public sealed class IngestorTests : IDisposable
{
    private const string LawFile = "ley-de-prueba.md";

    private readonly DirectoryInfo _workspace = Directory.CreateTempSubdirectory("ingestor-");
    private readonly string _corpusDirectory;
    private readonly string _indexPath;

    public IngestorTests()
    {
        _corpusDirectory = Path.Combine(_workspace.FullName, "corpus");
        _indexPath = Path.Combine(_workspace.FullName, "index", "articles.index.json");
        Directory.CreateDirectory(_corpusDirectory);
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus", LawFile),
            Path.Combine(_corpusDirectory, LawFile));
    }

    public void Dispose() => _workspace.Delete(recursive: true);

    [Fact]
    public async Task FirstRunEmbedsEveryFragmentAndWritesTheIndex()
    {
        var generator = new FakeEmbeddingGenerator(dimensions: 8);

        var summary = await CreateIngestor(generator).RunAsync();

        Assert.Equal((4, 3, 1, 4, 4, 0, 8), (summary.Articles, summary.InForce, summary.Repealed, summary.Fragments, summary.Embedded, summary.Reused, summary.Dimensions));
        var index = await IndexFile.ReadAsync(_indexPath);
        Assert.Equal("bge-m3", index.EmbeddingModel);
        Assert.Equal(8, index.Dimensions);
        Assert.Equal(["lp-1", "lp-2", "lp-3", "lp-4.1"], index.Articles.Select(a => a.Article.Id));
        Assert.All(index.Articles, a => Assert.Equal(8, a.Embedding.Length));
    }

    [Fact]
    public async Task EmbeddingInputIncludesLawAndArticleNumber()
    {
        var generator = new FakeEmbeddingGenerator();

        await CreateIngestor(generator).RunAsync();

        Assert.Contains("Ley de Prueba, artículo 1.\nOBJETO. Primer artículo de prueba.", generator.Inputs);
    }

    [Fact]
    public async Task SecondRunReusesUnchangedEmbeddings()
    {
        await CreateIngestor(new FakeEmbeddingGenerator()).RunAsync();
        var generator = new FakeEmbeddingGenerator();

        var summary = await CreateIngestor(generator).RunAsync();

        Assert.Equal((0, 4), (summary.Embedded, summary.Reused));
        Assert.Equal(0, generator.Calls);
    }

    [Fact]
    public async Task OnlyChangedArticlesAreEmbeddedAgain()
    {
        await CreateIngestor(new FakeEmbeddingGenerator()).RunAsync();
        var lawPath = Path.Combine(_corpusDirectory, LawFile);
        File.WriteAllText(lawPath, File.ReadAllText(lawPath).Replace("Segundo inciso.", "Segundo inciso modificado.", StringComparison.Ordinal));
        var generator = new FakeEmbeddingGenerator();

        var summary = await CreateIngestor(generator).RunAsync();

        Assert.Equal((1, 3), (summary.Embedded, summary.Reused));
        Assert.Contains("Segundo inciso modificado.", Assert.Single(generator.Inputs), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ChangingTheModelEmbedsEverythingAgain()
    {
        await CreateIngestor(new FakeEmbeddingGenerator()).RunAsync();

        var summary = await CreateIngestor(new FakeEmbeddingGenerator(), embeddingModel: "other-model").RunAsync();

        Assert.Equal((4, 0), (summary.Embedded, summary.Reused));
        Assert.Contains("other-model", summary.PreviousIndexWarning, StringComparison.Ordinal);
        Assert.Equal("other-model", (await IndexFile.ReadAsync(_indexPath)).EmbeddingModel);
    }

    [Fact]
    public async Task SendsFragmentsInBatches()
    {
        var lawPath = Path.Combine(_corpusDirectory, LawFile);
        var extra = string.Concat(Enumerable.Range(100, Ingestor.BatchSize).Select(n => $"\n## Artículo {n}\nTexto {n}.\n"));
        File.AppendAllText(lawPath, extra);
        var generator = new FakeEmbeddingGenerator();

        await CreateIngestor(generator).RunAsync();

        Assert.Equal(2, generator.Calls);
        Assert.Equal(4 + Ingestor.BatchSize, generator.Inputs.Count);
    }

    [Fact]
    public async Task InvalidCorpusFailsWithoutWritingTheIndex()
    {
        File.WriteAllText(Path.Combine(_corpusDirectory, "broken.md"), "## Artículo 1\nSin front matter.");
        var generator = new FakeEmbeddingGenerator();

        var exception = await Assert.ThrowsAsync<CorpusFormatException>(() => CreateIngestor(generator).RunAsync());

        Assert.Contains("broken.md", exception.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(_indexPath));
        Assert.Equal(0, generator.Calls);
    }

    [Fact]
    public async Task UnreadablePreviousIndexIsIgnoredWithAWarning()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_indexPath)!);
        File.WriteAllText(_indexPath, "{ not json");

        var summary = await CreateIngestor(new FakeEmbeddingGenerator()).RunAsync();

        Assert.Equal(4, summary.Embedded);
        Assert.Contains("Previous index ignored", summary.PreviousIndexWarning, StringComparison.Ordinal);
    }

    [Fact]
    public async Task IndexFileUsesSnakeCaseKeys()
    {
        await CreateIngestor(new FakeEmbeddingGenerator()).RunAsync();

        using var json = JsonDocument.Parse(File.ReadAllText(_indexPath));
        var root = json.RootElement;
        Assert.Equal("bge-m3", root.GetProperty("embedding_model").GetString());
        var first = root.GetProperty("articles")[0];
        Assert.True(first.TryGetProperty("content_hash", out _));
        Assert.True(first.GetProperty("article").TryGetProperty("article_number", out _));
        Assert.True(first.GetProperty("article").TryGetProperty("in_force", out _));
        Assert.False(first.GetProperty("article").TryGetProperty("article_id", out _));
    }

    private Ingestor CreateIngestor(FakeEmbeddingGenerator generator, string embeddingModel = "bge-m3") =>
        new(
            new CorpusParser(),
            generator,
            Options.Create(new OllamaOptions { Endpoint = "http://localhost:11434", EmbeddingModel = embeddingModel, ChatModel = "test" }),
            Options.Create(new CorpusOptions { Directory = _corpusDirectory, IndexPath = _indexPath, CutoffDate = new DateOnly(2026, 11, 19) }),
            TimeProvider.System);
}
