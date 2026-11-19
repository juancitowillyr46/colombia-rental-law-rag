using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RentalLawAssistant.Core.Options;

public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Binds the Ollama, Retrieval and Corpus sections and validates them when the host starts,
    /// so a bad configuration fails fast instead of on the first request.
    /// </summary>
    public static IServiceCollection AddRentalLawAssistantOptions(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RetrievalOptions>()
            .Bind(configuration.GetSection(RetrievalOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorpusOptions>()
            .Bind(configuration.GetSection(CorpusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
