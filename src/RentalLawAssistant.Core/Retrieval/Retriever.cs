using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;

namespace RentalLawAssistant.Core.Retrieval;

/// <summary>
/// Embeds the question with the same model as the ingestion, finds the closest in-force articles
/// and decides whether there is enough basis to answer at all.
/// </summary>
public sealed class Retriever(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IArticleStore store,
    IOptions<RetrievalOptions> options)
{
    /// <summary>
    /// Fragments fetched per requested article: a long article may occupy several of the top
    /// positions with its chunks, so extra candidates are fetched and then de-duplicated.
    /// </summary>
    public const int CandidatesPerArticle = 3;

    public async Task<RetrievalResult> RetrieveAsync(string question, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        var topK = options.Value.TopK;

        var embedding = await embeddingGenerator.GenerateVectorAsync(question, cancellationToken: cancellationToken);
        var candidates = await store.SearchAsync(embedding, topK * CandidatesPerArticle, inForceOnly: true, cancellationToken);

        // Best fragment per article, so the top K are K different articles.
        var articles = candidates
            .GroupBy(c => c.Article.ArticleId)
            .Select(group => group.First())
            .Take(topK)
            .ToList();

        var topSimilarity = articles.Count > 0 ? articles[0].Similarity : 0f;
        return new RetrievalResult(articles, topSimilarity, HasBasis: topSimilarity >= options.Value.MinSimilarity);
    }
}

/// <summary>
/// Retrieved articles (always returned, for debugging and evals) and whether the best one clears
/// <see cref="RetrievalOptions.MinSimilarity"/>. When <see cref="HasBasis"/> is false, the chat model is not called.
/// </summary>
public sealed record RetrievalResult(IReadOnlyList<ScoredArticle> Articles, float TopSimilarity, bool HasBasis);
