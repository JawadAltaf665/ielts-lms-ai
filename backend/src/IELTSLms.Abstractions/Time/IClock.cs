namespace IELTSLms.Abstractions.Time;

/// <summary>
/// Abstraction over the system clock so time-dependent logic stays testable
/// (Development Standards §10: no hidden statics like <c>DateTime.UtcNow</c> in domain/application code).
/// </summary>
public interface IClock
{
    /// <summary>The current instant in UTC.</summary>
    DateTimeOffset UtcNow { get; }
}
