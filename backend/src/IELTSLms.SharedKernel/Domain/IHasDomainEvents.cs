namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Non-generic view over an aggregate's recorded domain events. Lets the persistence layer collect
/// and clear events across aggregates without knowing each aggregate's identifier type.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Events raised during the current unit of work.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears recorded events after they have been dispatched.</summary>
    void ClearDomainEvents();
}
