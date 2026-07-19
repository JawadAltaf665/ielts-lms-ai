namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// The only entry point for modifying its cluster of entities; it enforces the consistency
/// boundary and records domain events for in-transaction dispatch (Development Standards §15,
/// Backend Architecture §9).
/// </summary>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    /// <summary>Events raised by this aggregate during the current unit of work.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Records a domain event to be dispatched when the aggregate is persisted.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears recorded events; called by the persistence layer after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
