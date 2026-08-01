namespace IELTSLms.Domain.Assessment;

// Strongly-typed identifiers for the Assessment bounded context (BC-13, ERD §6.4).

/// <summary>Strongly-typed identifier for a <see cref="MonthlyMockExam"/>.</summary>
public readonly record struct MockExamId(Guid Value)
{
    public static MockExamId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="MockExamResult"/>.</summary>
public readonly record struct MockExamResultId(Guid Value)
{
    public static MockExamResultId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
