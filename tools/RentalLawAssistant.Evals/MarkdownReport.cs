using System.Globalization;
using System.Text;

namespace RentalLawAssistant.Evals;

/// <summary>Builds the Markdown report: tuning and test side by side, then per-question detail and failures.</summary>
public static class MarkdownReport
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string Build(IReadOnlyList<QuestionResult> results, ReportSettings settings)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(settings);
        var tuning = results.Where(r => r.Question.Split == EvalQuestion.TuningSplit).ToList();
        var test = results.Where(r => r.Question.Split == EvalQuestion.TestSplit).ToList();
        var tuningMetrics = RetrievalMetrics.From(tuning);
        var testMetrics = RetrievalMetrics.From(test);
        var fullMode = settings.ChatModel is not null;
        var notRun = settings.IncludeTest ? "–" : "not run";

        var report = new StringBuilder();
        report.AppendLine("# Evaluation");
        report.AppendLine();
        report.AppendLine(Invariant, $"Mode: {(fullMode ? "full (retrieval + answer)" : "retrieval only")} · Embedding model: `{settings.EmbeddingModel}`{(fullMode ? $" · Chat model: `{settings.ChatModel}`" : string.Empty)} · Top K: {settings.TopK} · MinSimilarity: {Number(settings.MinSimilarity)} · Run: {settings.RunAt:yyyy-MM-dd HH:mm} UTC");
        report.AppendLine();
        if (!settings.IncludeTest)
        {
            report.AppendLine("Test split not run: it is held out and measured once, at the end, with `--final`.");
            report.AppendLine();
        }

        report.AppendLine("| Metric | Tuning | Test |");
        report.AppendLine("|---|---|---|");
        Row("Questions (answerable)", m => $"{m.Questions} ({m.Answerable})");
        Row($"Recall@{settings.TopK}", m => Percent(m.Recall));
        Row($"Hit rate@{settings.TopK}", m => Percent(m.HitRate));
        Row("MRR", m => Number(m.MeanReciprocalRank));
        Row("Correct rejection (threshold only)", m => Percent(m.RejectionAccuracy));
        if (fullMode)
        {
            Row("Correct rejection (end to end)", m => Percent(m.AnswerDecisionAccuracy));
            Row("Citation precision", m => Percent(m.CitationPrecision));
            Row("Citations discarded by validator", m => m.DiscardedCitations?.ToString(Invariant) ?? "–");
        }

        Row("Min top similarity, answerable", m => Number(m.MinTopSimilarityAnswerable));
        Row("Max top similarity, should reject", m => Number(m.MaxTopSimilarityUnanswerable));
        Row("Mean latency (s)", m => Number(m.MeanLatencySeconds));
        report.AppendLine();

        AppendThresholdSweep(report, tuning);
        AppendPerQuestion(report, results, fullMode);
        AppendFailures(report, results, fullMode);
        return report.ToString();

        void Row(string name, Func<RetrievalMetrics, string> value) =>
            report.AppendLine(Invariant, $"| {name} | {(tuning.Count == 0 ? "–" : value(tuningMetrics))} | {(test.Count == 0 ? notRun : value(testMetrics))} |");
    }

    private static void AppendThresholdSweep(StringBuilder report, List<QuestionResult> tuning)
    {
        var points = ThresholdPoint.Sweep(tuning);
        if (points.Count == 0)
        {
            return;
        }

        report.AppendLine("## Threshold sweep (tuning only)");
        report.AppendLine();
        report.AppendLine("Threshold-only decision: false reject = answerable question below the threshold; false accept = question that should be rejected at or above it.");
        report.AppendLine();
        report.AppendLine("| MinSimilarity | Accuracy | False rejects | False accepts |");
        report.AppendLine("|---|---|---|---|");
        foreach (var point in points.Where((_, i) => i % 2 == 0))
        {
            report.AppendLine(Invariant, $"| {Number(point.Threshold)} | {Percent(point.Accuracy)} | {point.FalseRejects} | {point.FalseAccepts} |");
        }

        report.AppendLine();
    }

    private static void AppendPerQuestion(StringBuilder report, IReadOnlyList<QuestionResult> results, bool fullMode)
    {
        report.AppendLine("## Per question");
        report.AppendLine();
        report.AppendLine(fullMode
            ? "| Id | Split | Category | Expected | Rank | Top similarity | Grounded | Cited | Latency (s) | Result |"
            : "| Id | Split | Category | Expected | Rank | Top similarity | Basis | Result |");
        report.AppendLine(fullMode ? "|---|---|---|---|---|---|---|---|---|---|" : "|---|---|---|---|---|---|---|---|");
        foreach (var result in results)
        {
            var expected = result.Question.ExpectedArticleIds.Count == 0 ? "(reject)" : string.Join(", ", result.Question.ExpectedArticleIds);
            var common = $"| {result.Question.Id} | {result.Question.Split} | {result.Question.Category} | {expected} | {result.FirstRelevantRank?.ToString(Invariant) ?? "–"} | {Number(result.Retrieval.TopSimilarity)}";
            report.AppendLine(fullMode
                ? $"{common} | {YesNo(result.Ask!.Grounded)} | {Cited(result)} | {Number(result.Latency.TotalSeconds)} | {Outcome(result)} |"
                : $"{common} | {YesNo(result.Retrieval.HasBasis)} | {Outcome(result)} |");
        }

        report.AppendLine();
    }

    private static void AppendFailures(StringBuilder report, IReadOnlyList<QuestionResult> results, bool fullMode)
    {
        report.AppendLine("## Failed questions");
        report.AppendLine();
        var failed = results.Where(r => r.Failed).ToList();
        if (failed.Count == 0)
        {
            report.AppendLine("None.");
        }

        foreach (var result in failed)
        {
            report.AppendLine(Invariant, $"### {result.Question.Id}: {result.Question.Question}");
            report.AppendLine();
            report.AppendLine(Invariant, $"Expected: {(result.Question.ShouldAnswer ? string.Join(", ", result.Question.ExpectedArticleIds) : "reject")} · Basis: {YesNo(result.Retrieval.HasBasis)}");
            report.AppendLine();
            foreach (var (article, rank) in result.Retrieval.Articles.Select((a, i) => (a, i + 1)))
            {
                var mark = result.Question.ExpectedArticleIds.Contains(article.Article.ArticleId) ? " ✓" : string.Empty;
                report.AppendLine(Invariant, $"{rank}. `{article.Article.Id}` {Number(article.Similarity)}{mark}");
            }

            if (fullMode)
            {
                report.AppendLine();
                report.AppendLine(Invariant, $"Answer (grounded: {YesNo(result.Ask!.Grounded)}, cited: {Cited(result)}): {result.Ask.Answer}");
            }

            report.AppendLine();
        }
    }

    private static string Cited(QuestionResult result) =>
        result.Ask!.Citations.Count == 0 ? "–" : string.Join(", ", result.Ask.Citations.Select(c => c.Id));

    private static string Outcome(QuestionResult result) => result.Failed ? "FAIL" : "ok";

    private static string YesNo(bool value) => value ? "yes" : "no";

    private static string Percent(double? value) => value?.ToString("P0", Invariant) ?? "–";

    private static string Number(double? value) => value?.ToString("0.000", Invariant) ?? "–";
}

/// <summary>Run settings printed in the report header. <see cref="ChatModel"/> is null in retrieval-only mode.</summary>
public sealed record ReportSettings(
    string EmbeddingModel,
    string? ChatModel,
    int TopK,
    float MinSimilarity,
    bool IncludeTest,
    DateTimeOffset RunAt);
