using System.Numerics.Tensors;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Indexing;

/// <summary>
/// Keeps the whole index in memory and scores every fragment with SIMD cosine similarity.
/// A brute-force scan over ~120 vectors of 1024 floats takes well under a millisecond.
/// </summary>
public sealed class InMemoryArticleStore : IArticleStore
{
    private const string ParagraphSeparator = "\n\n";

    private readonly IReadOnlyList<IndexedArticle> _fragments;
    private readonly Dictionary<string, LawArticle> _byId;
    private readonly int _dimensions;

    public InMemoryArticleStore(ArticleIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);
        if (index.Articles.Any(a => a.Embedding.Length != index.Dimensions))
        {
            throw new InvalidDataException($"All embeddings in the index must have {index.Dimensions} dimensions.");
        }

        _fragments = index.Articles;
        _dimensions = index.Dimensions;
        _byId = new Dictionary<string, LawArticle>(StringComparer.Ordinal);
        foreach (var fragment in _fragments)
        {
            _byId[fragment.Article.Id] = fragment.Article;
        }

        foreach (var chunks in _fragments.Select(f => f.Article).Where(a => a.Id != a.ArticleId).GroupBy(a => a.ArticleId))
        {
            var first = chunks.First();
            _byId[chunks.Key] = first with
            {
                Id = chunks.Key,
                Content = string.Join(ParagraphSeparator, chunks.Select(c => c.Content)),
            };
        }

        EmbeddingModel = index.EmbeddingModel;
    }

    /// <summary>Model that produced the stored embeddings; queries must use the same one.</summary>
    public string EmbeddingModel { get; }

    public int Count => _fragments.Count;

    /// <summary>
    /// Loads the index file and refuses to continue when it was built with a different embedding
    /// model: vectors from different models are not comparable, so results would be garbage.
    /// </summary>
    public static async Task<InMemoryArticleStore> LoadAsync(
        string indexPath, string expectedEmbeddingModel, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedEmbeddingModel);
        var index = await IndexFile.ReadAsync(indexPath, cancellationToken);
        if (!string.Equals(index.EmbeddingModel, expectedEmbeddingModel, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The index was built with embedding model '{index.EmbeddingModel}' but '{expectedEmbeddingModel}' is configured. " +
                "Run the ingestion again: dotnet run --project src/RentalLawAssistant.Ingestion");
        }

        return new InMemoryArticleStore(index);
    }

    public Task<IReadOnlyList<ScoredArticle>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, int topK, bool inForceOnly, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);
        if (queryEmbedding.Length != _dimensions)
        {
            throw new ArgumentException(
                $"The query embedding has {queryEmbedding.Length} dimensions; the index has {_dimensions}.",
                nameof(queryEmbedding));
        }

        var query = queryEmbedding.Span;
        var scored = new List<ScoredArticle>(_fragments.Count);
        foreach (var fragment in _fragments)
        {
            if (inForceOnly && !fragment.Article.InForce)
            {
                continue;
            }

            scored.Add(new ScoredArticle(fragment.Article, TensorPrimitives.CosineSimilarity(query, fragment.Embedding)));
        }

        IReadOnlyList<ScoredArticle> top = [.. scored.OrderByDescending(s => s.Similarity).Take(topK)];
        return Task.FromResult(top);
    }

    public Task<LawArticle?> GetByIdAsync(string id, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_byId.GetValueOrDefault(id));
    }
}
