namespace IELTSLms.Domain.Grammar;

// Strongly-typed identifiers for the Grammar bounded context (BC-07). Grouped in one file per the
// Billing convention (BillingIds.cs) — one-file-per-id doesn't scale without pure ceremony.

/// <summary>Strongly-typed identifier for a <see cref="GrammarTopic"/>.</summary>
public readonly record struct GrammarTopicId(Guid Value)
{
    public static GrammarTopicId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="GrammarExercise"/>.</summary>
public readonly record struct GrammarExerciseId(Guid Value)
{
    public static GrammarExerciseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="GrammarQuestion"/>.</summary>
public readonly record struct GrammarQuestionId(Guid Value)
{
    public static GrammarQuestionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="GrammarAttempt"/>.</summary>
public readonly record struct GrammarAttemptId(Guid Value)
{
    public static GrammarAttemptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="GrammarProgress"/>.</summary>
public readonly record struct GrammarProgressId(Guid Value)
{
    public static GrammarProgressId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
