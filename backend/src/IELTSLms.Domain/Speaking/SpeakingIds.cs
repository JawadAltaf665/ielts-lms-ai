namespace IELTSLms.Domain.Speaking;

// Strongly-typed identifiers for the Speaking bounded context (BC-13, ERD §6.3).

/// <summary>Strongly-typed identifier for a <see cref="SpeakingTask"/>.</summary>
public readonly record struct SpeakingTaskId(Guid Value)
{
    public static SpeakingTaskId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="SpeakingAttempt"/>.</summary>
public readonly record struct SpeakingAttemptId(Guid Value)
{
    public static SpeakingAttemptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="SpeakingEvaluation"/>.</summary>
public readonly record struct SpeakingEvaluationId(Guid Value)
{
    public static SpeakingEvaluationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
