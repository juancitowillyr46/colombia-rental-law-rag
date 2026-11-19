using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Messages;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Answering;

/// <summary>Retrieval → generation → validation. The only entry point callers need.</summary>
public sealed partial class AskService(
    Retriever retriever,
    AnswerGenerator generator,
    IArticleStore store,
    IOptions<CorpusOptions> corpusOptions,
    ILogger<AskService> logger)
{
    public async Task<AskResult> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        // 1. Retrieval. Below the threshold, decline without calling the chat model.
        var retrieval = await retriever.RetrieveAsync(question, cancellationToken);
        if (!retrieval.HasBasis)
        {
            LogBelowThreshold(logger, retrieval.TopSimilarity);
            return NoBasis(discardedCitations: 0);
        }

        // 2. Generation. A failed or malformed model answer is treated as "no basis".
        var answer = await generator.GenerateAsync(question, retrieval.Articles, cancellationToken);
        if (answer is null)
        {
            return NoBasis(discardedCitations: 0);
        }

        // 3. Validation: drop every citation that was not retrieved.
        var validation = CitationValidator.Validate(answer, retrieval.Articles);
        LogValidation(logger, answer.CitedArticleIds.Count, validation.DiscardedIds.Count, validation.IsGrounded);
        if (!validation.IsGrounded)
        {
            return NoBasis(validation.DiscardedIds.Count);
        }

        // 4. Citation text comes from the corpus, never from the model.
        var citations = new List<Citation>();
        foreach (var articleId in validation.ValidArticleIds)
        {
            var article = await store.GetByIdAsync(articleId, cancellationToken);
            if (article is { InForce: true })
            {
                citations.Add(new Citation(article.ArticleId, article.LawName, article.ArticleNumber, article.Content, article.SourceUrl));
            }
        }

        return citations.Count == 0
            ? NoBasis(validation.DiscardedIds.Count)
            : new AskResult(
                answer.Answer.Trim(),
                Grounded: true,
                citations,
                SpanishMessages.Disclaimer,
                corpusOptions.Value.CutoffDate,
                validation.DiscardedIds.Count);
    }

    private AskResult NoBasis(int discardedCitations) =>
        new(SpanishMessages.NoBasis, Grounded: false, [], SpanishMessages.Disclaimer, corpusOptions.Value.CutoffDate, discardedCitations);

    [LoggerMessage(Level = LogLevel.Information, Message = "No basis: top similarity {TopSimilarity:F3} is below the threshold; chat model not called")]
    private static partial void LogBelowThreshold(ILogger logger, float topSimilarity);

    [LoggerMessage(Level = LogLevel.Information, Message = "Citations: {Cited} cited, {Discarded} discarded; grounded: {Grounded}")]
    private static partial void LogValidation(ILogger logger, int cited, int discarded, bool grounded);
}
