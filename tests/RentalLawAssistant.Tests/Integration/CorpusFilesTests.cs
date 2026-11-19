using RentalLawAssistant.Core.Corpus;

namespace RentalLawAssistant.Tests.Integration;

/// <summary>Guards the real corpus in data/corpus: it must always parse and keep its key articles.</summary>
public sealed class CorpusFilesTests
{
    private static readonly IReadOnlyList<LawArticle> Articles =
        new CorpusParser().ParseDirectory(Path.Combine(FindRepositoryRoot(), "data", "corpus"));

    [Fact]
    public void ContainsTheExpectedLaws()
    {
        Assert.Equal(["cc", "d1077", "l820"], Articles.Select(a => a.LawId).Distinct().Order());
        Assert.Equal(63, ArticleIds("cc").Count);
        Assert.Equal(43, ArticleIds("l820").Count);
        Assert.Single(ArticleIds("d1077"));
    }

    [Theory]
    [InlineData("cc-1985")]
    [InlineData("cc-1993")]
    [InlineData("cc-1998")]
    [InlineData("cc-2028")]
    [InlineData("cc-2029")]
    [InlineData("l820-27")]
    [InlineData("d1077-2.2.6.1.1.10")]
    public void KeyRepairArticlesAreInForce(string articleId)
    {
        Assert.All(Articles.Where(a => a.ArticleId == articleId), a => Assert.True(a.InForce));
        Assert.Contains(Articles, a => a.ArticleId == articleId);
    }

    [Fact]
    public void RepealedArticlesAreMarked()
    {
        var repealed = Articles.Where(a => !a.InForce).Select(a => a.ArticleId).Distinct().Order();

        Assert.Equal(["cc-2035", "l820-35", "l820-36", "l820-38", "l820-39", "l820-40"], repealed);
    }

    [Fact]
    public void NoFragmentContainsPortalLeftovers()
    {
        Assert.All(Articles, a => Assert.DoesNotMatch(@"[<>]|&\w+;|Notas de Vigencia", a.Content));
    }

    private static HashSet<string> ArticleIds(string lawId) =>
        [.. Articles.Where(a => a.LawId == lawId).Select(a => a.ArticleId)];

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RentalLawAssistant.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repository root (RentalLawAssistant.slnx) not found.");
    }
}
