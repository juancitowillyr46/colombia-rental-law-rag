using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Retrieval;
using RentalLawAssistant.Evals;

namespace RentalLawAssistant.Tests.Unit;

public sealed class RetrievalMetricsTests
{
    [Fact]
    public void ScoresRankRecallAndReciprocalRank()
    {
        var result = Result(["a-1", "a-9"], shouldAnswer: true, hasBasis: true, retrieved: ["a-5", "a-1#2", "a-3"]);

        Assert.Equal(2, result.FirstRelevantRank);
        Assert.Equal(0.5, result.Recall);
        Assert.Equal(0.5, result.ReciprocalRank);
        Assert.False(result.Failed);
    }

    [Fact]
    public void MissingTheExpectedArticleIsAFailure()
    {
        var result = Result(["a-1"], shouldAnswer: true, hasBasis: true, retrieved: ["a-2", "a-3"]);

        Assert.Null(result.FirstRelevantRank);
        Assert.Equal(0, result.ReciprocalRank);
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    public void RejectionIsCorrectOnlyWhenTheDecisionMatches(bool shouldAnswer, bool hasBasis, bool expected)
    {
        var result = Result(shouldAnswer ? ["a-1"] : [], shouldAnswer, hasBasis, retrieved: ["a-1"]);

        Assert.Equal(expected, result.RejectionCorrect);
    }

    [Fact]
    public void AggregatesOnlyAnswerableQuestionsForRecallAndMrr()
    {
        QuestionResult[] results =
        [
            Result(["a-1"], shouldAnswer: true, hasBasis: true, retrieved: ["a-1"], topSimilarity: 0.8f),
            Result(["a-2"], shouldAnswer: true, hasBasis: true, retrieved: ["a-9", "a-2"], topSimilarity: 0.6f),
            Result([], shouldAnswer: false, hasBasis: true, retrieved: ["a-1"], topSimilarity: 0.55f),
        ];

        var metrics = RetrievalMetrics.From(results);

        Assert.Equal((3, 2), (metrics.Questions, metrics.Answerable));
        Assert.Equal(1.0, metrics.Recall);
        Assert.Equal(1.0, metrics.HitRate);
        Assert.Equal(0.75, metrics.MeanReciprocalRank);
        Assert.Equal(2.0 / 3, metrics.RejectionAccuracy, precision: 6);
        Assert.Equal(0.6f, metrics.MinTopSimilarityAnswerable);
        Assert.Equal(0.55f, metrics.MaxTopSimilarityUnanswerable);
    }

    [Fact]
    public void RealQuestionsFileIsValid()
    {
        var path = Path.Combine(FindRepositoryRoot(), "evals", "questions.json");

        var questions = EvalQuestion.Load(path);

        Assert.True(questions.Count >= 15);
        Assert.Contains(questions, q => !q.ShouldAnswer);
    }

    [Fact]
    public void RejectsQuestionsWhoseExpectationContradictsShouldAnswer()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
            [{ "id": "q1", "split": "tuning", "category": "clear", "question": "¿?", "expected_article_ids": [], "expected_answer": "", "should_answer": true }]
            """);
        try
        {
            var exception = Assert.Throws<InvalidDataException>(() => EvalQuestion.Load(path));

            Assert.Contains("'q1'", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static QuestionResult Result(
        string[] expected, bool shouldAnswer, bool hasBasis, string[] retrieved, float topSimilarity = 0.7f)
    {
        var question = new EvalQuestion("q", EvalQuestion.TuningSplit, "clear", "¿Pregunta?", expected, string.Empty, shouldAnswer);
        var articles = retrieved
            .Select(id => new ScoredArticle(
                new LawArticle(id, "a", "Ley", id.Split('-', '#')[1], "Texto.", InForce: true, "https://example.org"),
                topSimilarity))
            .ToList();
        return new QuestionResult(question, new RetrievalResult(articles, topSimilarity, hasBasis), TimeSpan.FromMilliseconds(10));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RentalLawAssistant.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
