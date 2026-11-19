using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Indexing;

/// <summary>
/// Vector search over the corpus. Stage 1 keeps it in memory; stage 2 will implement the same
/// interface on PostgreSQL + pgvector without touching the callers.
/// </summary>
public interface IArticleStore
{
    /// <summary>Returns up to <paramref name="topK"/> fragments ordered by descending cosine similarity.</summary>
    Task<IReadOnlyList<ScoredArticle>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, int topK, bool inForceOnly, CancellationToken ct);

    /// <summary>
    /// Looks up a fragment id ("l820-15#2") or a whole article id ("l820-15"). For an article split
    /// into chunks, the article id returns the full text with its chunks joined in order.
    /// </summary>
    Task<LawArticle?> GetByIdAsync(string id, CancellationToken ct);
}
