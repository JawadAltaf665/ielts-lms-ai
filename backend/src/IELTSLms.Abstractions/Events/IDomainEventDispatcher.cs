using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Abstractions.Events;

/// <summary>
/// Publishes domain events to their handlers. Invoked by the persistence layer inside the same
/// transaction as <c>SaveChanges</c> so handlers observe a consistent state
/// (Backend Architecture §9, Development Standards §15). Keeps the domain free of any messaging SDK.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Dispatches the given domain events to their registered handlers.</summary>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
