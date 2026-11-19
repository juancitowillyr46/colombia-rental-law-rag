using RentalLawAssistant.Core.Corpus;

namespace RentalLawAssistant.Core.Indexing;

/// <summary>A corpus fragment with its embedding, as stored in the index file.</summary>
/// <param name="Article">The fragment exactly as parsed from the corpus.</param>
/// <param name="ContentHash">SHA-256 (hex) of the text sent to the embedding model; unchanged hash = reuse the embedding.</param>
/// <param name="EmbeddingModel">Model that produced <paramref name="Embedding"/>, e.g. "bge-m3".</param>
/// <param name="Embedding">The embedding vector.</param>
public sealed record IndexedArticle(
    LawArticle Article,
    string ContentHash,
    string EmbeddingModel,
    float[] Embedding);
