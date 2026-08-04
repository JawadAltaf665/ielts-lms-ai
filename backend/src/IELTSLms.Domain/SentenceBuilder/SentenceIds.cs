namespace IELTSLms.Domain.SentenceBuilder;

// Strongly-typed identifiers for the Sentence Builder bounded context (Phase 7 Sprint 7, ERD ENT-019/
// ENT-020). Grouped in one file per the Billing/Grammar convention.

/// <summary>Strongly-typed identifier for a <see cref="SentenceExercise"/>.</summary>
public readonly record struct SentenceExerciseId(Guid Value)
{
    public static SentenceExerciseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="SentenceAttempt"/>.</summary>
public readonly record struct SentenceAttemptId(Guid Value)
{
    public static SentenceAttemptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
