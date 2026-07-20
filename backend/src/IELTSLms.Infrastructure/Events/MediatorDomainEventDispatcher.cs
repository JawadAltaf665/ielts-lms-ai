using System.Collections.Concurrent;
using IELTSLms.Abstractions.Events;
using IELTSLms.SharedKernel.Domain;
using MediatR;

namespace IELTSLms.Infrastructure.Events;

/// <summary>
/// Dispatches domain events by wrapping each in a <see cref="DomainEventNotification{T}"/> and
/// publishing it through MediatR. Called by the persistence layer inside the save transaction.
/// </summary>
internal sealed class MediatorDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    // Cache the closed generic notification type per event type to avoid repeated reflection.
    private static readonly ConcurrentDictionary<Type, Func<IDomainEvent, INotification>> NotificationFactories = new();

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notification = Wrap(domainEvent);
            await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private static INotification Wrap(IDomainEvent domainEvent)
    {
        var factory = NotificationFactories.GetOrAdd(domainEvent.GetType(), CreateFactory);
        return factory(domainEvent);
    }

    private static Func<IDomainEvent, INotification> CreateFactory(Type eventType)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
        var constructor = notificationType.GetConstructors()[0];
        return domainEvent => (INotification)constructor.Invoke([domainEvent]);
    }
}
