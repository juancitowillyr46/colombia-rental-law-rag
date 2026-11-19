using System.Text.Json;
using RentalLawAssistant.Core.Indexing;

namespace RentalLawAssistant.Evals;

/// <summary>One entry of evals/questions.json.</summary>
/// <param name="Id">"q001".</param>
/// <param name="Split">"tuning" (used to adjust the system) or "test" (measured once, at the end).</param>
/// <param name="Category">clear, everyday_words, out_of_corpus or repealed; only used to read the results.</param>
/// <param name="Question">The question as a user would ask it.</param>
/// <param name="ExpectedArticleIds">Article ids that answer it ("cc-1985"); empty when it should be rejected.</param>
/// <param name="ExpectedAnswer">Reference answer for the full evals (phase 3); may be empty.</param>
/// <param name="ShouldAnswer">False when the corpus has no basis and the assistant must decline.</param>
public sealed record EvalQuestion(
    string Id,
    string Split,
    string Category,
    string Question,
    IReadOnlyList<string> ExpectedArticleIds,
    string ExpectedAnswer,
    bool ShouldAnswer)
{
    public const string TuningSplit = "tuning";
    public const string TestSplit = "test";

    public static IReadOnlyList<EvalQuestion> Load(string path)
    {
        var questions = JsonSerializer.Deserialize<List<EvalQuestion>>(File.ReadAllText(path), IndexFile.SerializerOptions)
            ?? throw new InvalidDataException($"{path} is empty.");

        var duplicate = questions.GroupBy(q => q.Id).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidDataException($"{path}: question id '{duplicate.Key}' appears more than once.");
        }

        var invalid = questions.FirstOrDefault(q =>
            q.Split is not (TuningSplit or TestSplit)
            || string.IsNullOrWhiteSpace(q.Question)
            || q.ShouldAnswer == (q.ExpectedArticleIds.Count == 0));
        return invalid is null
            ? questions
            : throw new InvalidDataException(
                $"{path}: question '{invalid.Id}' is invalid. Split must be tuning or test, and expected_article_ids must be empty exactly when should_answer is false.");
    }
}
