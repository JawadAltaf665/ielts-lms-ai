namespace IELTSLms.Domain.Vocabulary;

// Strongly-typed identifiers for the Vocabulary bounded context (BC-08), grouped in one file per the
// Grammar/Billing convention (GrammarIds.cs) — one-file-per-id doesn't scale without pure ceremony.

/// <summary>Strongly-typed identifier for a <see cref="VocabularyWord"/>.</summary>
public readonly record struct VocabularyWordId(Guid Value)
{
    public static VocabularyWordId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="VocabularyProgress"/>.</summary>
public readonly record struct VocabularyProgressId(Guid Value)
{
    public static VocabularyProgressId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
