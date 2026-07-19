using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>Raised when repeated failed logins trip the account lockout (Auth §6).</summary>
/// <param name="UserId">The locked account.</param>
/// <param name="LockoutEndsAtUtc">When the time-boxed lockout auto-expires (UTC).</param>
public sealed record UserLockedOutDomainEvent(Guid UserId, DateTimeOffset LockoutEndsAtUtc) : DomainEvent;
