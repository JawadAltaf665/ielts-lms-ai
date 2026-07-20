using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>Raised on a successful password authentication.</summary>
/// <param name="UserId">The authenticated user.</param>
/// <param name="SessionId">The device session opened for this login.</param>
public sealed record UserLoggedInDomainEvent(Guid UserId, Guid SessionId) : DomainEvent;
