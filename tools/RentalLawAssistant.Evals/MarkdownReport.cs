using System.Globalization;
using System.Text;

namespace RentalLawAssistant.Evals;

/// <summary>Builds the Markdown report: tuning and test side by side, then every failed question.</summary>
public static class MarkdownReport
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string Build(
        IReadOnlyList<QuestionResult> results, string embeddingModel, int topK, float minSimilarity, DateTimeOffset runAt)
    {
        ArgumentNullException.ThrowIfNull(results);
        var tuning = results.Where(r => r.Question.Split == EvalQuestion.TuningSplit).ToList();
        var test = results.Where(r => r.Question.Split == EvalQuestion.TestSplit).ToList();
        var tuningMetrics = RetrievalMetrics.From(tuning);
        var testMetrics = RetrievalMetrics.From(test);

        var report = new StringBuilder();
        report.AppendLine("# Retrieval evaluation");
        report.AppendLine();
        report.AppendLine(Invariant, $"Mode: retrieval only · Embedding model: `{embeddingModel}` · Top K: {topK} · MinSimilarity: {Number(minSimilarity)} · Run: {runAt:yyyy-MM-dd HH:mm} UTC");
        report.AppendLine();
        report.AppendLine("| Metric | Tuning | Test |");
        report.AppendLine("|---|---|---|");
        Row("Questions (answerable)", m => $"{m.Questions} ({m.Answerable})");
        Row($"Recall@{topK}", m => Percent(m.Recall));
        Row($"Hit rate@{topK}", m => Percent(m.HitRate));
        Row("MRR", m => Number(m.MeanReciprocalRank));
        Row("Correct rejection", m => Percent(m.RejectionAccuracy));
        Row("Min top similarity, answerable", m => Number(m.MinTopSimilarityAnswerable));
        Row("Max top similarity, should reject", m => Number(m.MaxTopSimilarityUnanswerable));
        Row("Mean latency (s)", m => Number(m.MeanLatencySeconds));
        report.AppendLine();

        report.AppendLine("## Per question");
        report.AppendLine();
        report.AppendLine("| Id | Split | Category | Expected | Rank | Top similarity | Basis | Result |");
        report.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var result in results)
        {
            var expected = result.Question.ExpectedArticleIds.Count == 0 ? "(reject)" : string.Join(", ", result.Question.ExpectedArticleIds);
            report.AppendLine(Invariant,
                $"| {result.Question.Id} | {result.Question.Split} | {result.Question.Category} | {expected} | {result.FirstRelevantRank?.ToString(Invariant) ?? "–"} | {Number(result.Retrieval.TopSimilarity)} | {(result.Retrieval.HasBasis ? "yes" : "no")} | {(result.Failed ? "FAIL" : "ok")} |");
        }

        report.AppendLine();
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
            report.AppendLine(Invariant, $"Expected: {(result.Question.ShouldAnswer ? string.Join(", ", result.Question.ExpectedArticleIds) : "reject")} · Basis: {(result.Retrieval.HasBasis ? "yes" : "no")}");
            report.AppendLine();
            foreach (var (article, rank) in result.Retrieval.Articles.Select((a, i) => (a, i + 1)))
            {
                var mark = result.Question.ExpectedArticleIds.Contains(article.Article.ArticleId) ? " ✓" : string.Empty;
                report.AppendLine(Invariant, $"{rank}. `{article.Article.Id}` {Number(article.Similarity)}{mark}");
            }

            report.AppendLine();
        }

        return report.ToString();

        void Row(string name, Func<RetrievalMetrics, string> value) =>
            report.AppendLine(Invariant, $"| {name} | {Cell(tuning, tuningMetrics, value)} | {Cell(test, testMetrics, value)} |");
    }

    private static string Cell(List<QuestionResult> split, RetrievalMetrics metrics, Func<RetrievalMetrics, string> value) =>
        split.Count == 0 ? "–" : value(metrics);

    private static string Percent(double value) => value.ToString("P0", Invariant);

    private static string Number(double? value) => value?.ToString("0.000", Invariant) ?? "–";
}
