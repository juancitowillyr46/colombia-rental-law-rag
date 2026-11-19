namespace RentalLawAssistant.Core.Answering;

/// <summary>Final answer returned to callers (API, evals).</summary>
/// <param name="Answer">The model's answer, or <see cref="Messages.SpanishMessages.NoBasis"/>.</param>
/// <param name="Grounded">False whenever the assistant declined to answer.</param>
/// <param name="Citations">Validated citations; their text always comes from the corpus.</param>
/// <param name="Disclaimer">Legal disclaimer in Spanish.</param>
/// <param name="CorpusCutoff">Date up to which the legal texts were checked.</param>
/// <param name="DiscardedCitations">How many model citations were dropped by the validator (a metric, not shown to users).</param>
public sealed record AskResult(
    string Answer,
    bool Grounded,
    IReadOnlyList<Citation> Citations,
    string Disclaimer,
    DateOnly CorpusCutoff,
    int DiscardedCitations);

/// <summary>An article cited in the answer, with its exact text from the corpus.</summary>
public sealed record Citation(string Id, string Law, string Article, string Text, string SourceUrl);
