namespace IELTSLms.Domain.Learning;

/// <summary>Strongly-typed identifier for a <see cref="ProgressTracking"/>.</summary>
public readonly record struct ProgressTrackingId(Guid Value)
{
    public static ProgressTrackingId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
