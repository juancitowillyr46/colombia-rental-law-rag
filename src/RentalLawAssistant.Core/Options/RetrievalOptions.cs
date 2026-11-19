using System.ComponentModel.DataAnnotations;

namespace RentalLawAssistant.Core.Options;

/// <summary>How many articles to retrieve and the minimum similarity to answer at all.</summary>
public sealed class RetrievalOptions
{
    public const string SectionName = "Retrieval";

    [Range(1, 50)]
    public int TopK { get; set; } = 5;

    /// <summary>Cosine similarity of the best match below which the assistant answers "no basis".</summary>
    [Range(0.0, 1.0)]
    public float MinSimilarity { get; set; } = 0.5f;
}
