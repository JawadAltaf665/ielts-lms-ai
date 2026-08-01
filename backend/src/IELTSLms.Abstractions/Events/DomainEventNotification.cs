using IELTSLms.SharedKernel.Domain;
using MediatR;

namespace IELTSLms.Abstractions.Events;

/// <summary>
/// MediatR envelope around an <see cref="IDomainEvent"/>. This keeps the domain free of any
/// dependency on MediatR: aggregates raise plain domain events, and the dispatcher wraps them in
/// this notification so application-layer handlers can subscribe via <c>INotificationHandler&lt;&gt;</c>.
/// Lives in Abstractions (not Infrastructure) so Application-layer handlers can reference it without
/// violating the golden dependency rule (Backend Architecture §5).
/// </summary>
/// <typeparam name="TDomainEvent">The wrapped domain event type.</typeparam>
/// <param name="DomainEvent">The domain event being published.</param>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
