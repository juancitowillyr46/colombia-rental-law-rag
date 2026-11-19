using System.ComponentModel;
using RentalLawAssistant.Core.Answering;

namespace RentalLawAssistant.Api.Contracts;

/// <summary>Body of <c>POST /ask</c>.</summary>
public sealed record AskRequest(
    [property: Description("Question in Spanish, 1 to 500 characters. Example: \"Se dañó el calentador por viejo, ¿lo paga el inquilino o el propietario?\"")]
    string? Question);

/// <summary>Response of <c>POST /ask</c> (camelCase JSON).</summary>
public sealed record AskResponse(
    [property: Description("Short answer in Spanish, or the no-basis message when grounded is false.")]
    string Answer,
    [property: Description("False when the assistant found no basis in the corpus and declined to answer.")]
    bool Grounded,
    [property: Description("Validated citations. Empty when grounded is false.")]
    IReadOnlyList<CitationDto> Citations,
    [property: Description("Legal disclaimer in Spanish.")]
    string Disclaimer,
    [property: Description("Date up to which the legal texts were checked (yyyy-MM-dd).")]
    DateOnly CorpusCutoff)
{
    public static AskResponse From(AskResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new AskResponse(
            result.Answer,
            result.Grounded,
            [.. result.Citations.Select(c => new CitationDto(c.Id, c.Law, c.Article, c.Text, c.SourceUrl))],
            result.Disclaimer,
            result.CorpusCutoff);
    }
}

/// <summary>A cited article; <see cref="Text"/> is the exact corpus text.</summary>
public sealed record CitationDto(
    [property: Description("Article id, e.g. \"cc-1985\" or \"l820-27\".")]
    string Id,
    [property: Description("Law name, e.g. \"Código Civil\".")]
    string Law,
    [property: Description("Article number, e.g. \"1985\".")]
    string Article,
    [property: Description("Exact article text from the corpus, never written by the model.")]
    string Text,
    [property: Description("Official source of the law.")]
    string SourceUrl);

/// <summary>Response of <c>GET /ask/debug</c> (Development only): what retrieval found and why.</summary>
public sealed record DebugResponse(
    string Question,
    [property: Description("Cosine similarity of the best article.")]
    float TopSimilarity,
    [property: Description("Configured threshold; below it the chat model is not called.")]
    float MinSimilarity,
    bool HasBasis,
    IReadOnlyList<DebugArticleDto> Articles);

public sealed record DebugArticleDto(
    [property: Description("Fragment id; long articles are split into chunks such as \"l820-15#2\".")]
    string Id,
    [property: Description("Whole-article id used for citations.")]
    string ArticleId,
    string Law,
    string Article,
    float Similarity);
