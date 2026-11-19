using RentalLawAssistant.Core.Corpus;

namespace RentalLawAssistant.Tests.Unit;

public sealed class CorpusParserTests
{
    private const string FrontMatter = """
        ---
        law_id: lp
        law_name: Ley de Prueba
        source_url: https://example.org/ley-de-prueba
        ---

        """;

    private static readonly string FixtureDirectory = Path.Combine(AppContext.BaseDirectory, "Fixtures", "corpus");

    [Fact]
    public void ParsesFrontMatterAndArticles()
    {
        var articles = new CorpusParser().ParseDirectory(FixtureDirectory);

        Assert.Equal(["lp-1", "lp-2", "lp-3", "lp-4.1"], articles.Select(a => a.Id));
        var first = articles[0];
        Assert.Equal("lp", first.LawId);
        Assert.Equal("Ley de Prueba", first.LawName);
        Assert.Equal("1", first.ArticleNumber);
        Assert.Equal("OBJETO. Primer artículo de prueba.", first.Content);
        Assert.Equal("https://example.org/ley-de-prueba", first.SourceUrl);
        Assert.True(first.InForce);
    }

    [Fact]
    public void KeepsParagraphsAndDropsNoteComments()
    {
        var article = new CorpusParser().ParseDirectory(FixtureDirectory).Single(a => a.Id == "lp-2");

        Assert.Equal("Primer inciso.\n\nSegundo inciso.", article.Content);
    }

    [Fact]
    public void MarksRepealedArticlesAsNotInForce()
    {
        var article = new CorpusParser().ParseDirectory(FixtureDirectory).Single(a => a.Id == "lp-3");

        Assert.False(article.InForce);
        Assert.Equal("Artículo derogado por la ley de prueba.", article.Content);
    }

    [Fact]
    public void HandlesWindowsLineEndings()
    {
        var markdown = (FrontMatter + "## Artículo 1\nUno.\n\nDos.\n").ReplaceLineEndings("\r\n");

        var article = Assert.Single(new CorpusParser().Parse(markdown, "crlf.md"));

        Assert.Equal("Uno.\n\nDos.", article.Content);
    }

    [Fact]
    public void SplitsLongArticlesByParagraphKeepingTheArticleId()
    {
        var paragraphs = Enumerable.Range(1, 5).Select(i => $"Inciso {i} " + new string('x', 30)).ToList();
        var markdown = FrontMatter + "## Artículo 7\n" + string.Join("\n\n", paragraphs);

        var chunks = new CorpusParser(maxChunkLength: 100).Parse(markdown, "long.md");

        Assert.Equal(["lp-7#1", "lp-7#2", "lp-7#3"], chunks.Select(c => c.Id));
        Assert.All(chunks, c => Assert.Equal("lp-7", c.ArticleId));
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 100));
        Assert.Equal(string.Join("\n\n", paragraphs), string.Join("\n\n", chunks.Select(c => c.Content)));
    }

    [Fact]
    public void DoesNotSplitArticlesWithinTheLimit()
    {
        var article = Assert.Single(new CorpusParser().Parse(FrontMatter + "## Artículo 7\nCorto.", "short.md"));

        Assert.Equal("lp-7", article.Id);
        Assert.Equal(article.Id, article.ArticleId);
    }

    [Theory]
    [InlineData("## Artículo 1\nUno.", "front matter")]
    [InlineData("---\nlaw_id: lp\nlaw_name: Ley\n---\n## Artículo 1\nUno.", "source_url")]
    [InlineData("---\nlaw_id: lp\nlaw_name: Ley\nsource_url: https://x\n## Artículo 1\nUno.", "not closed")]
    [InlineData("---\nlaw_id: lp\nlaw_name: Ley\nsource_url: https://x\n---\nSin encabezado.", "before the first")]
    [InlineData("---\nlaw_id: lp\nlaw_name: Ley\nsource_url: https://x\n---\n", "no '## Artículo' headings")]
    public void RejectsInvalidFiles(string markdown, string expectedMessage)
    {
        var exception = Assert.Throws<CorpusFormatException>(() => new CorpusParser().Parse(markdown, "bad.md"));

        Assert.Contains("bad.md", exception.Message, StringComparison.Ordinal);
        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("## Artículo 1\n<!-- note: only a comment -->\n", "has no content")]
    [InlineData("## Artículo 1\nUno.\n\n## Artículo 1\nOtra vez.", "appears twice")]
    [InlineData("## Artículo 1\n<!-- status: suspended -->\nUno.", "unknown status 'suspended'")]
    [InlineData("## Articulo uno\nUno.", "invalid heading")]
    public void RejectsInvalidArticles(string articles, string expectedMessage)
    {
        var exception = Assert.Throws<CorpusFormatException>(
            () => new CorpusParser().Parse(FrontMatter + articles, "bad.md"));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsDuplicateIdsAcrossFiles()
    {
        var directory = Directory.CreateTempSubdirectory("corpus-");
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "a.md"), FrontMatter + "## Artículo 1\nUno.");
            File.WriteAllText(Path.Combine(directory.FullName, "b.md"), FrontMatter + "## Artículo 1\nOtro.");

            var exception = Assert.Throws<CorpusFormatException>(
                () => new CorpusParser().ParseDirectory(directory.FullName));

            Assert.Contains("Duplicate article id 'lp-1' in a.md and b.md", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void RejectsMissingDirectory()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var exception = Assert.Throws<CorpusFormatException>(() => new CorpusParser().ParseDirectory(missing));

        Assert.Contains("not found", exception.Message, StringComparison.Ordinal);
    }
}
