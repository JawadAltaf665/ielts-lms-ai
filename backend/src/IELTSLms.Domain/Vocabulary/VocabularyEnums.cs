namespace IELTSLms.Domain.Vocabulary;

/// <summary>
/// Difficulty tier of a <see cref="VocabularyWord"/> (ERD ENT-017). Modeled as an enum field rather
/// than a lookup table per the sprint's reconciliation notes — same convention as
/// <c>GrammarDifficulty</c> on <c>GrammarTopic</c>; promote to a table only if a future sprint needs
/// tenant-customizable difficulty tiers.
/// </summary>
public enum VocabularyDifficultyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
}

/// <summary>How commonly a <see cref="VocabularyWord"/> occurs in IELTS material (ERD ENT-017 metadata).</summary>
public enum VocabularyFrequencyBand
{
    High = 1,
    Medium = 2,
    Low = 3,
}
