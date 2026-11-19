using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core.Answering;

/// <summary>
/// Asks the chat model for a structured <see cref="ModelAnswer"/>. Any failure (model down, timeout,
/// invalid JSON) is logged and returned as null, so the caller answers "no basis" instead of
/// showing a raw error to the user.
/// </summary>
public sealed partial class AnswerGenerator(IChatClient chatClient, ILogger<AnswerGenerator> logger)
{
    /// <summary>Low temperature for stable, repeatable answers.</summary>
    public const float Temperature = 0.1f;

    public async Task<ModelAnswer?> GenerateAsync(
        string question, IReadOnlyList<ScoredArticle> articles, CancellationToken cancellationToken = default)
    {
        ChatMessage[] messages =
        [
            new(ChatRole.System, Prompts.System),
            new(ChatRole.User, Prompts.BuildUserPrompt(question, articles)),
        ];
        var options = new ChatOptions { Temperature = Temperature };

        try
        {
            var response = await chatClient.GetResponseAsync<ModelAnswer>(
                messages, options, useJsonSchemaResponseFormat: true, cancellationToken);

            if (response.TryGetResult(out var answer) && answer is { Answer: not null, CitedArticleIds: not null })
            {
                return answer;
            }

            LogInvalidAnswer(logger, Truncate(response.Text));
            return null;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException or TaskCanceledException
            && !cancellationToken.IsCancellationRequested)
        {
            LogModelFailure(logger, exception);
            return null;
        }
    }

    private static string Truncate(string text) => text.Length <= 500 ? text : text[..500] + "…";

    [LoggerMessage(Level = LogLevel.Error, Message = "The chat model returned an invalid answer: {Response}")]
    private static partial void LogInvalidAnswer(ILogger logger, string response);

    [LoggerMessage(Level = LogLevel.Error, Message = "The chat model call failed")]
    private static partial void LogModelFailure(ILogger logger, Exception exception);
}
