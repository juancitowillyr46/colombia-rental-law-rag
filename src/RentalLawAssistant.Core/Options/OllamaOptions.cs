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
}
