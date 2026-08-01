using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Application.Vocabulary;

/// <summary>
/// Configuration-driven review-batch sizing (Sprint-2-Vocabulary-Module.md business rules: "Review-batch
/// size default/max are configurable, not hardcoded — mirrors pagination limit default/max convention").
/// </summary>
public sealed class VocabularyOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Vocabulary";

    /// <summary>Batch size used when the caller does not request one.</summary>
    [Range(1, 200)]
    public int DefaultReviewBatchSize { get; set; } = 20;

    /// <summary>Maximum batch size a caller may request.</summary>
    [Range(1, 200)]
    public int MaxReviewBatchSize { get; set; } = 50;
}
