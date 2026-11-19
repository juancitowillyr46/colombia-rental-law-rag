using RentalLawAssistant.Core.Corpus;

namespace RentalLawAssistant.Core.Retrieval;

/// <summary>A corpus fragment and its cosine similarity to the question (-1 to 1, higher is closer).</summary>
public sealed record ScoredArticle(LawArticle Article, float Similarity);
