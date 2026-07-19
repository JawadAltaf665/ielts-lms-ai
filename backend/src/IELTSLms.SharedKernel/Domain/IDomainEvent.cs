namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marker for something that has happened in the domain. Named in the past tense and raised by
/// aggregates, then dispatched in-transaction (Development Standards §4, §15).
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique id of this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>When the event occurred (UTC).</summary>
    DateTimeOffset OccurredOnUtc { get; }
}
