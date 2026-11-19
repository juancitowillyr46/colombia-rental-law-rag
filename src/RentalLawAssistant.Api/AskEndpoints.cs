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
        app.MapPost("/ask", AskAsync);

        // Shows the retrieved articles and their similarity; useful to debug and to record the demo.
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/ask/debug", DebugAsync);
        }

        return app;
    }

    private static async Task<IResult> AskAsync(AskRequest request, AskService askService, CancellationToken cancellationToken)
    {
        if (ValidateQuestion(request.Question) is { } error)
        {
            return error;
        }

        var result = await askService.AskAsync(request.Question!.Trim(), cancellationToken);
        return Results.Ok(AskResponse.From(result));
    }

    private static async Task<IResult> DebugAsync(
        string? question, Retriever retriever, IOptions<RetrievalOptions> options, CancellationToken cancellationToken)
    {
        if (ValidateQuestion(question) is { } error)
        {
            return error;
        }

        var retrieval = await retriever.RetrieveAsync(question!.Trim(), cancellationToken);
        return Results.Ok(new DebugResponse(
            question.Trim(),
            retrieval.TopSimilarity,
            options.Value.MinSimilarity,
            retrieval.HasBasis,
            [.. retrieval.Articles.Select(a => new DebugArticleDto(
                a.Article.Id, a.Article.ArticleId, a.Article.LawName, a.Article.ArticleNumber, a.Similarity))]));
    }

    private static IResult? ValidateQuestion(string? question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return Results.Problem(SpanishMessages.QuestionRequired, statusCode: StatusCodes.Status400BadRequest);
        }

        return question.Trim().Length > MaxQuestionLength
            ? Results.Problem(SpanishMessages.QuestionTooLong(MaxQuestionLength), statusCode: StatusCodes.Status400BadRequest)
            : null;
    }
}
