namespace RentalLawAssistant.Core.Answering;

/// <summary>
/// What the chat model must return (structured output). Nothing here is trusted as-is:
/// <see cref="CitationValidator"/> decides which citations survive.
/// </summary>
/// <param name="Answer">Short answer in Spanish.</param>
/// <param name="CitedArticleIds">Ids of the provided articles that support the answer, e.g. "cc-1985".</param>
/// <param name="Grounded">False when the provided articles do not answer the question.</param>
public sealed record ModelAnswer(string Answer, IReadOnlyList<string> CitedArticleIds, bool Grounded);
