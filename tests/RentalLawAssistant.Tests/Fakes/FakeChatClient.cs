using System.Text.Json;
using Microsoft.Extensions.AI;

namespace RentalLawAssistant.Tests.Fakes;

/// <summary>
/// Stand-in for the chat model: replies with a scripted text (usually JSON) and records every call,
/// so tests can prove when the model was or was not called. It can also invent citations or fail.
/// </summary>
public sealed class FakeChatClient(Func<IReadOnlyList<ChatMessage>, string> reply) : IChatClient
{
    public FakeChatClient(string reply)
        : this(_ => reply)
    {
    }

    public int Calls { get; private set; }

    public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];

    public ChatOptions? LastOptions { get; private set; }

    /// <summary>When set, every call throws this exception (e.g. Ollama not running).</summary>
    public Exception? Failure { get; set; }

    /// <summary>JSON in the shape of <c>ModelAnswer</c>.</summary>
    public static string Answer(string answer, bool grounded, params string[] citedArticleIds) =>
        JsonSerializer.Serialize(new { answer, citedArticleIds, grounded });

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Calls++;
        LastMessages = [.. messages];
        LastOptions = options;
        if (Failure is not null)
        {
            throw Failure;
        }

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply(LastMessages))));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Streaming is not used.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
