namespace IELTSLms.Domain.Reading;

// Strongly-typed identifiers for the Reading bounded context (BC-10, ERD §6.3).

/// <summary>Strongly-typed identifier for a <see cref="ReadingPassage"/>.</summary>
public readonly record struct ReadingPassageId(Guid Value)
{
    public static ReadingPassageId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ReadingQuestion"/>.</summary>
public readonly record struct ReadingQuestionId(Guid Value)
{
    public static ReadingQuestionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ReadingAttempt"/>.</summary>
public readonly record struct ReadingAttemptId(Guid Value)
{
    public static ReadingAttemptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
