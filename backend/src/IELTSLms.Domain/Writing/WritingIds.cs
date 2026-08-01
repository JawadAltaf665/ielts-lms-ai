namespace IELTSLms.Domain.Writing;

// Strongly-typed identifiers for the Writing bounded context (BC-12, ERD §6.3).

/// <summary>Strongly-typed identifier for a <see cref="WritingTask"/>.</summary>
public readonly record struct WritingTaskId(Guid Value)
{
    public static WritingTaskId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="WritingSubmission"/>.</summary>
public readonly record struct WritingSubmissionId(Guid Value)
{
    public static WritingSubmissionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="WritingEvaluation"/>.</summary>
public readonly record struct WritingEvaluationId(Guid Value)
{
    public static WritingEvaluationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
