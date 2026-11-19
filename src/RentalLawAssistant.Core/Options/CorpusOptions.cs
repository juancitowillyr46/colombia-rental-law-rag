using System.ComponentModel.DataAnnotations;

namespace RentalLawAssistant.Core.Options;

/// <summary>Location of the corpus and the generated index, plus the corpus cutoff date.</summary>
public sealed class CorpusOptions : IValidatableObject
{
    public const string SectionName = "Corpus";

    [Required]
    public string Directory { get; set; } = string.Empty;

    [Required]
    public string IndexPath { get; set; } = string.Empty;

    /// <summary>Date up to which the legal texts were checked; shown to the user with every answer.</summary>
    public DateOnly CutoffDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CutoffDate == default)
        {
            yield return new ValidationResult(
                "The CutoffDate field is required (format yyyy-MM-dd).",
                [nameof(CutoffDate)]);
        }
    }
}
