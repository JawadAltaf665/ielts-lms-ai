using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>Raised when a user confirms their email address and the account becomes active.</summary>
/// <param name="UserId">The user whose email was verified.</param>
public sealed record UserEmailVerifiedDomainEvent(Guid UserId) : DomainEvent;
