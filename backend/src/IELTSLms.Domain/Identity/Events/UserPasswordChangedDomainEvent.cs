using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>
/// Raised when a user's password changes (via reset). Consumers revoke existing sessions and
/// notify the user (Auth §5: credential change invalidates sessions).
/// </summary>
/// <param name="UserId">The user whose password changed.</param>
public sealed record UserPasswordChangedDomainEvent(Guid UserId) : DomainEvent;
