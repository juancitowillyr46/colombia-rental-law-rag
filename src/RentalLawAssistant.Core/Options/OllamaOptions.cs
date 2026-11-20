using System.ComponentModel.DataAnnotations;

namespace RentalLawAssistant.Core.Options;

/// <summary>Connection to the local Ollama server and the models it serves.</summary>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Must match the model recorded in the index file.</summary>
    [Required]
    public string EmbeddingModel { get; set; } = string.Empty;

    [Required]
    public string ChatModel { get; set; } = string.Empty;

    /// <summary>
    /// HTTP timeout per Ollama call. On CPU, the first call to a large model also loads it into memory,
    /// which can exceed HttpClient's 100-second default.
    /// </summary>
    [Range(10, 3600)]
    public int RequestTimeoutSeconds { get; set; } = 300;

    /// <summary>Creates the HttpClient that the Ollama client should use, with this endpoint and timeout.</summary>
    public HttpClient CreateHttpClient() => new()
    {
        BaseAddress = new Uri(Endpoint),
        Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds),
    };
}
