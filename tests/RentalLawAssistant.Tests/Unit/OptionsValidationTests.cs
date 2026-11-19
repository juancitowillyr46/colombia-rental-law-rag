using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RentalLawAssistant.Core.Options;

namespace RentalLawAssistant.Tests.Unit;

public sealed class OptionsValidationTests
{
    private static Dictionary<string, string?> ValidSettings() => new()
    {
        ["Ollama:Endpoint"] = "http://localhost:11434",
        ["Ollama:EmbeddingModel"] = "bge-m3",
        ["Ollama:ChatModel"] = "qwen2.5:3b",
        ["Retrieval:TopK"] = "5",
        ["Retrieval:MinSimilarity"] = "0.5",
        ["Corpus:Directory"] = "data/corpus",
        ["Corpus:IndexPath"] = "data/index/articles.index.json",
        ["Corpus:CutoffDate"] = "2026-11-19",
    };

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection()
            .AddRentalLawAssistantOptions(configuration)
            .BuildServiceProvider();
    }

    [Fact]
    public void ValidSettingsAreBound()
    {
        using var provider = BuildProvider(ValidSettings());

        provider.GetRequiredService<IStartupValidator>().Validate();

        Assert.Equal("bge-m3", provider.GetRequiredService<IOptions<OllamaOptions>>().Value.EmbeddingModel);
        Assert.Equal(0.5f, provider.GetRequiredService<IOptions<RetrievalOptions>>().Value.MinSimilarity);
        Assert.Equal(new DateOnly(2026, 11, 19), provider.GetRequiredService<IOptions<CorpusOptions>>().Value.CutoffDate);
    }

    [Theory]
    [InlineData("Ollama:Endpoint", "", "Endpoint")]
    [InlineData("Ollama:Endpoint", "not-a-url", "Endpoint")]
    [InlineData("Ollama:EmbeddingModel", "", "EmbeddingModel")]
    [InlineData("Retrieval:TopK", "0", "TopK")]
    [InlineData("Retrieval:MinSimilarity", "1.5", "MinSimilarity")]
    [InlineData("Corpus:IndexPath", "", "IndexPath")]
    [InlineData("Corpus:CutoffDate", null, "CutoffDate")]
    public void InvalidSettingFailsAtStartup(string key, string? value, string expectedField)
    {
        var settings = ValidSettings();
        settings[key] = value;
        using var provider = BuildProvider(settings);

        // A single failing options type is thrown as-is (several would be wrapped in AggregateException).
        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.Contains(expectedField, exception.Message, StringComparison.Ordinal);
    }
}
