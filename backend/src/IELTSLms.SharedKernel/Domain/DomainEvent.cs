namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Base record for domain events; supplies the event id and occurrence timestamp.
/// Concrete events are past-tense records, e.g. <c>WritingSubmitted</c>.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredOnUtc { get; init; }
}
