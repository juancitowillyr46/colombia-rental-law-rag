using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Evals;

/// <summary>
/// What the system returned for one question, and how it scores against the expectation.
/// <see cref="Ask"/> is null in retrieval-only mode.
/// </summary>
public sealed record QuestionResult(EvalQuestion Question, RetrievalResult Retrieval, TimeSpan Latency, AskResult? Ask = null)
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

    /// <summary>End to end (full mode): the final answer is grounded exactly when it should be.</summary>
    public bool? AnswerDecisionCorrect => Ask is null ? null : Ask.Grounded == Question.ShouldAnswer;

    /// <summary>Full mode: share of final citations that are expected articles; null when nothing was cited.</summary>
    public double? CitationPrecision => Ask is { Citations.Count: > 0 }
        ? Ask.Citations.Count(c => Question.ExpectedArticleIds.Contains(c.Id)) / (double)Ask.Citations.Count
        : null;

    public bool Failed =>
        Ask is null
            ? !RejectionCorrect || (Question.ShouldAnswer && FirstRelevantRank is null)
            : AnswerDecisionCorrect == false || CitationPrecision == 0;
}

/// <summary>Aggregated metrics for one split.</summary>
public sealed record RetrievalMetrics(
    int Questions,
    int Answerable,
    double Recall,
    double HitRate,
    double MeanReciprocalRank,
    double RejectionAccuracy,
    double MeanLatencySeconds,
    float? MinTopSimilarityAnswerable,
    float? MaxTopSimilarityUnanswerable,
    double? AnswerDecisionAccuracy,
    double? CitationPrecision,
    int? DiscardedCitations)
{
    public static RetrievalMetrics From(IReadOnlyCollection<QuestionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var answerable = results.Where(r => r.Question.ShouldAnswer).ToList();
        var unanswerable = results.Where(r => !r.Question.ShouldAnswer).ToList();
        var full = results.Where(r => r.Ask is not null).ToList();
        var withCitations = full.Where(r => r.CitationPrecision is not null).ToList();

        return new RetrievalMetrics(
            Questions: results.Count,
            Answerable: answerable.Count,
            Recall: Average(answerable, r => r.Recall),
            HitRate: Average(answerable, r => r.FirstRelevantRank is null ? 0 : 1),
            MeanReciprocalRank: Average(answerable, r => r.ReciprocalRank),
            RejectionAccuracy: Average(results, r => r.RejectionCorrect ? 1 : 0),
            MeanLatencySeconds: Average(results, r => r.Latency.TotalSeconds),
            MinTopSimilarityAnswerable: answerable.Count > 0 ? answerable.Min(r => r.Retrieval.TopSimilarity) : null,
            MaxTopSimilarityUnanswerable: unanswerable.Count > 0 ? unanswerable.Max(r => r.Retrieval.TopSimilarity) : null,
            AnswerDecisionAccuracy: full.Count > 0 ? Average(full, r => r.AnswerDecisionCorrect == true ? 1 : 0) : null,
            CitationPrecision: withCitations.Count > 0 ? Average(withCitations, r => r.CitationPrecision!.Value) : null,
            DiscardedCitations: full.Count > 0 ? full.Sum(r => r.Ask!.DiscardedCitations) : null);
    }

    private static double Average(IReadOnlyCollection<QuestionResult> results, Func<QuestionResult, double> selector) =>
        results.Count == 0 ? 0 : results.Average(selector);
}

/// <summary>How the threshold-only decision would score at a given MinSimilarity (tuning data only).</summary>
public sealed record ThresholdPoint(float Threshold, double Accuracy, int FalseRejects, int FalseAccepts)
{
    /// <summary>Evaluates every threshold from <paramref name="from"/> to <paramref name="to"/> in steps of <paramref name="step"/>.</summary>
    public static IReadOnlyList<ThresholdPoint> Sweep(IReadOnlyCollection<QuestionResult> results, float from = 0.35f, float to = 0.65f, float step = 0.01f)
    {
        ArgumentNullException.ThrowIfNull(results);
        var points = new List<ThresholdPoint>();
        if (results.Count == 0)
        {
            return points;
        }

        var steps = (int)Math.Round((to - from) / step);
        for (var i = 0; i <= steps; i++)
        {
            var threshold = MathF.Round(from + (i * step), 2);
            var falseRejects = results.Count(r => r.Question.ShouldAnswer && r.Retrieval.TopSimilarity < threshold);
            var falseAccepts = results.Count(r => !r.Question.ShouldAnswer && r.Retrieval.TopSimilarity >= threshold);
            points.Add(new ThresholdPoint(threshold, 1 - ((falseRejects + falseAccepts) / (double)results.Count), falseRejects, falseAccepts));
        }

        return points;
    }
}
