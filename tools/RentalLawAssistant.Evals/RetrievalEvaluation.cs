using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Evals;

/// <summary>What retrieval returned for one question, and how it scores against the expectation.</summary>
public sealed record QuestionResult(EvalQuestion Question, RetrievalResult Retrieval, TimeSpan Latency)
{
    public IReadOnlyList<string> RetrievedArticleIds { get; } =
        [.. Retrieval.Articles.Select(a => a.Article.ArticleId)];

    /// <summary>1-based position of the first expected article in the results, or null if none was retrieved.</summary>
    public int? FirstRelevantRank
    {
        get
        {
            for (var i = 0; i < RetrievedArticleIds.Count; i++)
            {
                if (Question.ExpectedArticleIds.Contains(RetrievedArticleIds[i]))
                {
                    return i + 1;
                }
            }

            return null;
        }
    }

    /// <summary>Share of the expected articles that were retrieved (0 to 1).</summary>
    public double Recall => Question.ExpectedArticleIds.Count == 0
        ? 0
        : Question.ExpectedArticleIds.Count(RetrievedArticleIds.Contains) / (double)Question.ExpectedArticleIds.Count;

    public double ReciprocalRank => FirstRelevantRank is { } rank ? 1.0 / rank : 0;

    /// <summary>The threshold decision matches the expectation: answer when it should, decline when it should not.</summary>
    public bool RejectionCorrect => Retrieval.HasBasis == Question.ShouldAnswer;

    public bool Failed => !RejectionCorrect || (Question.ShouldAnswer && FirstRelevantRank is null);
}

/// <summary>Aggregated retrieval metrics for one split.</summary>
public sealed record RetrievalMetrics(
    int Questions,
    int Answerable,
    double Recall,
    double HitRate,
    double MeanReciprocalRank,
    double RejectionAccuracy,
    double MeanLatencySeconds,
    float? MinTopSimilarityAnswerable,
    float? MaxTopSimilarityUnanswerable)
{
    public static RetrievalMetrics From(IReadOnlyCollection<QuestionResult> results)
    {
        var answerable = results.Where(r => r.Question.ShouldAnswer).ToList();
        var unanswerable = results.Where(r => !r.Question.ShouldAnswer).ToList();
        return new RetrievalMetrics(
            Questions: results.Count,
            Answerable: answerable.Count,
            Recall: Average(answerable, r => r.Recall),
            HitRate: Average(answerable, r => r.FirstRelevantRank is null ? 0 : 1),
            MeanReciprocalRank: Average(answerable, r => r.ReciprocalRank),
            RejectionAccuracy: Average(results, r => r.RejectionCorrect ? 1 : 0),
            MeanLatencySeconds: Average(results, r => r.Latency.TotalSeconds),
            MinTopSimilarityAnswerable: answerable.Count > 0 ? answerable.Min(r => r.Retrieval.TopSimilarity) : null,
            MaxTopSimilarityUnanswerable: unanswerable.Count > 0 ? unanswerable.Max(r => r.Retrieval.TopSimilarity) : null);
    }

    private static double Average(IReadOnlyCollection<QuestionResult> results, Func<QuestionResult, double> selector) =>
        results.Count == 0 ? 0 : results.Average(selector);
}
