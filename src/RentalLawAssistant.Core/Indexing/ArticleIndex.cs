namespace RentalLawAssistant.Core.Indexing;

/// <summary>Contents of data/index/articles.index.json.</summary>
/// <param name="EmbeddingModel">Model used for every embedding in the file; the API refuses to start on a mismatch.</param>
/// <param name="Dimensions">Length of every embedding vector (1024 for bge-m3).</param>
/// <param name="CreatedAt">When the ingestion wrote the file.</param>
/// <param name="Articles">All corpus fragments, in corpus order.</param>
public sealed record ArticleIndex(
    string EmbeddingModel,
    int Dimensions,
    DateTimeOffset CreatedAt,
    IReadOnlyList<IndexedArticle> Articles);
