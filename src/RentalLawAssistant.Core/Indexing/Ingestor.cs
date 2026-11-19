using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Corpus;
using RentalLawAssistant.Core.Options;

namespace RentalLawAssistant.Core.Indexing;

/// <summary>
/// Builds the index file: parse corpus → hash each fragment → embed only new or changed fragments → save.
/// </summary>
public sealed class Ingestor(
    CorpusParser parser,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IOptions<OllamaOptions> ollamaOptions,
    IOptions<CorpusOptions> corpusOptions,
    TimeProvider timeProvider)
{
    /// <summary>Fragments sent to the embedding model per request.</summary>
    public const int BatchSize = 16;

    public async Task<IngestionSummary> RunAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = timeProvider.GetTimestamp();
        var model = ollamaOptions.Value.EmbeddingModel;
        var corpusDirectory = RepositoryPaths.Resolve(corpusOptions.Value.Directory);
        var indexPath = RepositoryPaths.Resolve(corpusOptions.Value.IndexPath);

        // 1-2. Parse and validate the corpus; throws CorpusFormatException before anything is written.
        var articles = parser.ParseDirectory(corpusDirectory);

        // 3. Reuse embeddings whose text and model did not change.
        var (reusable, previousIndexWarning) = await LoadReusableEmbeddingsAsync(indexPath, model, cancellationToken);
        var fragments = articles
            .Select(article =>
            {
                var input = BuildEmbeddingInput(article);
                return new Fragment(article, input, ComputeHash(input));
            })
            .ToList();

        var embeddings = new float[]?[fragments.Count];
        var pending = new List<int>();
        for (var i = 0; i < fragments.Count; i++)
        {
            if (reusable.TryGetValue(fragments[i].Article.Id, out var previous) && previous.ContentHash == fragments[i].Hash)
            {
                embeddings[i] = previous.Embedding;
            }
            else
            {
                pending.Add(i);
            }
        }

        // 4. Embed the rest in batches.
        foreach (var batch in pending.Chunk(BatchSize))
        {
            var generated = await embeddingGenerator.GenerateAsync(
                batch.Select(i => fragments[i].Input), cancellationToken: cancellationToken);
            if (generated.Count != batch.Length)
            {
                throw new InvalidOperationException(
                    $"The embedding model returned {generated.Count} vectors for {batch.Length} inputs.");
            }

            for (var j = 0; j < batch.Length; j++)
            {
                embeddings[batch[j]] = generated[j].Vector.ToArray();
            }
        }

        var dimensions = embeddings[0]!.Length;
        if (dimensions == 0 || embeddings.Any(e => e!.Length != dimensions))
        {
            throw new InvalidOperationException("Embeddings must be non-empty and all of the same length.");
        }

        // 5. Save the index and report.
        var indexed = fragments
            .Select((fragment, i) => new IndexedArticle(fragment.Article, fragment.Hash, model, embeddings[i]!))
            .ToList();
        await IndexFile.WriteAsync(indexPath, new ArticleIndex(model, dimensions, timeProvider.GetUtcNow(), indexed), cancellationToken);

        var wholeArticles = articles.DistinctBy(a => a.ArticleId).ToList();
        return new IngestionSummary(
            IndexPath: indexPath,
            Articles: wholeArticles.Count,
            InForce: wholeArticles.Count(a => a.InForce),
            Repealed: wholeArticles.Count(a => !a.InForce),
            Fragments: fragments.Count,
            Embedded: pending.Count,
            Reused: fragments.Count - pending.Count,
            Dimensions: dimensions,
            Elapsed: timeProvider.GetElapsedTime(startedAt),
            PreviousIndexWarning: previousIndexWarning);
    }

    /// <summary>
    /// Text sent to the embedding model: law and article number give context to chunks that,
    /// on their own, do not say which article they belong to. Questions are embedded as plain text.
    /// </summary>
    public static string BuildEmbeddingInput(LawArticle article)
    {
        ArgumentNullException.ThrowIfNull(article);
        return $"{article.LawName}, artículo {article.ArticleNumber}.\n{article.Content}";
    }

    public static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static async Task<(Dictionary<string, IndexedArticle> Reusable, string? Warning)> LoadReusableEmbeddingsAsync(
        string indexPath, string model, CancellationToken cancellationToken)
    {
        if (!File.Exists(indexPath))
        {
            return ([], null);
        }

        ArticleIndex previous;
        try
        {
            previous = await IndexFile.ReadAsync(indexPath, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            return ([], $"Previous index ignored: {exception.Message}");
        }

        if (previous.EmbeddingModel != model)
        {
            return ([], $"Previous index used model '{previous.EmbeddingModel}'; re-embedding everything with '{model}'.");
        }

        return (previous.Articles.Where(a => a.EmbeddingModel == model).ToDictionary(a => a.Article.Id), null);
    }

    private sealed record Fragment(LawArticle Article, string Input, string Hash);
}

/// <summary>What the ingestion did; printed by the console app.</summary>
public sealed record IngestionSummary(
    string IndexPath,
    int Articles,
    int InForce,
    int Repealed,
    int Fragments,
    int Embedded,
    int Reused,
    int Dimensions,
    TimeSpan Elapsed,
    string? PreviousIndexWarning);
