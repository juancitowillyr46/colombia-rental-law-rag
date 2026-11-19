using RentalLawAssistant.Core.Answering;

namespace RentalLawAssistant.Api.Contracts;

/// <summary>Body of <c>POST /ask</c>.</summary>
public sealed record AskRequest(string? Question);

/// <summary>Response of <c>POST /ask</c> (camelCase JSON).</summary>
public sealed record AskResponse(
    string Answer,
    bool Grounded,
    IReadOnlyList<CitationDto> Citations,
    string Disclaimer,
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
public sealed record CitationDto(string Id, string Law, string Article, string Text, string SourceUrl);

/// <summary>Response of <c>GET /ask/debug</c> (Development only): what retrieval found and why.</summary>
public sealed record DebugResponse(
    string Question,
    float TopSimilarity,
    float MinSimilarity,
    bool HasBasis,
    IReadOnlyList<DebugArticleDto> Articles);

public sealed record DebugArticleDto(string Id, string ArticleId, string Law, string Article, float Similarity);
