using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Tests.Unit;

public sealed class CitationValidatorTests
{
    private static readonly IReadOnlyList<ScoredArticle> Retrieved =
    [
        Scored("cc-1985", "1985"),
        Scored("cc-2028", "2028"),
        Scored("l820-15#2", "15"),
    ];

    [Fact]
    public void KeepsCitationsThatWereRetrieved()
    {
        var validation = CitationValidator.Validate(new ModelAnswer("Respuesta.", ["cc-1985", "cc-2028"], Grounded: true), Retrieved);

        Assert.Equal(["cc-1985", "cc-2028"], validation.ValidArticleIds);
        Assert.Empty(validation.DiscardedIds);
        Assert.True(validation.IsGrounded);
    }

    [Fact]
    public void DropsInventedCitations()
    {
        var validation = CitationValidator.Validate(new ModelAnswer("Respuesta.", ["cc-1985", "cc-9999", "l820-27"], Grounded: true), Retrieved);

        Assert.Equal(["cc-1985"], validation.ValidArticleIds);
        Assert.Equal(["cc-9999", "l820-27"], validation.DiscardedIds);
        Assert.True(validation.IsGrounded);
    }

    [Fact]
    public void AllInventedCitationsMeansNotGrounded()
    {
        var validation = CitationValidator.Validate(new ModelAnswer("Respuesta.", ["cc-9999"], Grounded: true), Retrieved);

        Assert.Empty(validation.ValidArticleIds);
        Assert.False(validation.IsGrounded);
    }

    [Fact]
    public void NoCitationsMeansNotGrounded()
    {
        var validation = CitationValidator.Validate(new ModelAnswer("Respuesta.", [], Grounded: true), Retrieved);

        Assert.False(validation.IsGrounded);
    }

    [Fact]
    public void ModelSayingNotGroundedWinsEvenWithValidCitations()
    {
        var validation = CitationValidator.Validate(new ModelAnswer("No sé.", ["cc-1985"], Grounded: false), Retrieved);

        Assert.False(validation.IsGrounded);
    }

    [Fact]
    public void NormalizesChunkIdsBracketsAndDuplicates()
    {
        var validation = CitationValidator.Validate(
            new ModelAnswer("Respuesta.", ["l820-15#1", "[l820-15]", " cc-1985 ", "cc-1985"], Grounded: true), Retrieved);

        Assert.Equal(["l820-15", "cc-1985"], validation.ValidArticleIds);
        Assert.Empty(validation.DiscardedIds);
    }

    [Fact]
    public void NeverAcceptsRepealedArticlesEvenIfRetrieved()
    {
        IReadOnlyList<ScoredArticle> retrieved = [Scored("cc-2035", "2035", inForce: false)];

        var validation = CitationValidator.Validate(new ModelAnswer("Respuesta.", ["cc-2035"], Grounded: true), retrieved);

        Assert.False(validation.IsGrounded);
        Assert.Equal(["cc-2035"], validation.DiscardedIds);
    }

    private static ScoredArticle Scored(string id, string number, bool inForce = true) =>
        new(new LawArticle(id, id.Split('-')[0], "Ley", number, "Texto.", inForce, "https://example.org"), 0.8f);
}
