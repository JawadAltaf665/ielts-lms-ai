namespace IELTSLms.Domain.AI;

/// <summary>Strongly-typed identifier for a <see cref="LessonSession"/>.</summary>
public readonly record struct LessonSessionId(Guid Value)
{
    public static LessonSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
