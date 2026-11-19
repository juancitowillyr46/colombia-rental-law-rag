using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Api.Contracts;
using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Messages;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Api;

/// <summary>Thin HTTP layer: validate input, call Core, map to contracts.</summary>
public static class AskEndpoints
{
    public const int MaxQuestionLength = 500;

    public static WebApplication MapAskEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapPost("/ask", AskAsync)
            .WithName("Ask")
            .WithTags("Ask")
            .WithSummary("Answer a residential rental law question")
            .WithDescription(
                "Retrieves the closest in-force articles, asks the chat model for a short answer in Spanish and keeps only " +
                "citations that were retrieved. Citation texts always come from the corpus. When there is no basis, " +
                "returns 200 with grounded = false and no citations.")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // Shows the retrieved articles and their similarity; useful to debug and to record the demo.
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/ask/debug", DebugAsync)
                .WithName("AskDebug")
                .WithTags("Debug")
                .WithSummary("Show what retrieval found (Development only)")
                .WithDescription("Returns the top articles with their cosine similarity and whether they clear MinSimilarity. Does not call the chat model.")
                .ProducesProblem(StatusCodes.Status400BadRequest);
        }

        return app;
    }

    private static async Task<Results<Ok<AskResponse>, ProblemHttpResult>> AskAsync(
        AskRequest request, AskService askService, CancellationToken cancellationToken)
    {
        if (ValidateQuestion(request.Question) is { } error)
        {
            return error;
        }

        var result = await askService.AskAsync(request.Question!.Trim(), cancellationToken);
        return TypedResults.Ok(AskResponse.From(result));
    }

    private static async Task<Results<Ok<DebugResponse>, ProblemHttpResult>> DebugAsync(
        string? question, Retriever retriever, IOptions<RetrievalOptions> options, CancellationToken cancellationToken)
    {
        if (ValidateQuestion(question) is { } error)
        {
            return error;
        }

        var retrieval = await retriever.RetrieveAsync(question!.Trim(), cancellationToken);
        return TypedResults.Ok(new DebugResponse(
            question.Trim(),
            retrieval.TopSimilarity,
            options.Value.MinSimilarity,
            retrieval.HasBasis,
            [.. retrieval.Articles.Select(a => new DebugArticleDto(
                a.Article.Id, a.Article.ArticleId, a.Article.LawName, a.Article.ArticleNumber, a.Similarity))]));
    }

    private static ProblemHttpResult? ValidateQuestion(string? question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return TypedResults.Problem(SpanishMessages.QuestionRequired, statusCode: StatusCodes.Status400BadRequest);
        }

        return question.Trim().Length > MaxQuestionLength
            ? TypedResults.Problem(SpanishMessages.QuestionTooLong(MaxQuestionLength), statusCode: StatusCodes.Status400BadRequest)
            : null;
    }
}
