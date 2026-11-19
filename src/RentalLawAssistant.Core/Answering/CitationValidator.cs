using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Answering;

/// <summary>
/// The golden rule in code: the model writes, the code decides what is valid. A citation survives
/// only if its article was among the retrieved (in-force) articles for this question.
/// </summary>
public static class CitationValidator
{
    public static CitationValidation Validate(ModelAnswer answer, IReadOnlyList<ScoredArticle> retrieved)
    {
        ArgumentNullException.ThrowIfNull(answer);
        ArgumentNullException.ThrowIfNull(retrieved);

        var allowed = retrieved
            .Where(r => r.Article.InForce)
            .Select(r => r.Article.ArticleId)
            .ToHashSet(StringComparer.Ordinal);

        var valid = new List<string>();
        var discarded = new List<string>();
        foreach (var cited in answer.CitedArticleIds.Distinct(StringComparer.Ordinal))
        {
            // The model may echo a chunk id ("l820-15#2"); citations always point to the whole article.
            var articleId = ToArticleId(cited);
            if (allowed.Contains(articleId))
            {
                if (!valid.Contains(articleId))
                {
                    valid.Add(articleId);
                }
            }
            else
            {
                discarded.Add(cited);
            }
        }

        return new CitationValidation(valid, discarded, IsGrounded: answer.Grounded && valid.Count > 0);
    }

    private static string ToArticleId(string cited)
    {
        var trimmed = cited.Trim().Trim('[', ']');
        var hash = trimmed.IndexOf('#', StringComparison.Ordinal);
        return hash < 0 ? trimmed : trimmed[..hash];
    }
}

/// <summary>Result of validating the model's citations against the retrieved set.</summary>
/// <param name="ValidArticleIds">Cited article ids that were retrieved, in the model's order, without duplicates.</param>
/// <param name="DiscardedIds">Cited ids that were not retrieved (invented or out of context).</param>
/// <param name="IsGrounded">True only if the model said grounded and at least one citation is valid.</param>
public sealed record CitationValidation(
    IReadOnlyList<string> ValidArticleIds,
    IReadOnlyList<string> DiscardedIds,
    bool IsGrounded);
