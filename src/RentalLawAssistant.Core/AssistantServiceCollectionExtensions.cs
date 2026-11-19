using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Answering;
using RentalLawAssistant.Core.Indexing;
using RentalLawAssistant.Core.Options;
using RentalLawAssistant.Core.Retrieval;

namespace RentalLawAssistant.Core;

public static class AssistantServiceCollectionExtensions
{
    /// <summary>
    /// Registers retrieval, generation and validation. The host must also register an
    /// <c>IEmbeddingGenerator&lt;string, Embedding&lt;float&gt;&gt;</c> and an <c>IChatClient</c> (Ollama in production, fakes in tests).
    /// </summary>
    public static IServiceCollection AddRentalLawAssistant(this IServiceCollection services)
    {
        // Loaded once and kept in memory. Fails (and stops the host) if the index is missing or was
        // built with a different embedding model than the configured one.
        services.AddSingleton<IArticleStore>(provider =>
        {
            var corpus = provider.GetRequiredService<IOptions<CorpusOptions>>().Value;
            var ollama = provider.GetRequiredService<IOptions<OllamaOptions>>().Value;
            return InMemoryArticleStore
                .LoadAsync(RepositoryPaths.Resolve(corpus.IndexPath), ollama.EmbeddingModel)
                .GetAwaiter()
                .GetResult();
        });
        services.AddSingleton<Retriever>();
        services.AddSingleton<AnswerGenerator>();
        services.AddSingleton<AskService>();
        return services;
    }
}
