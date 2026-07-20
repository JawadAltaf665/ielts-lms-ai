using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>Raised when a password authentication attempt fails against an existing account.</summary>
/// <param name="UserId">The account the failed attempt targeted.</param>
/// <param name="FailedAttemptCount">The running count of consecutive failed attempts.</param>
public sealed record UserLoginFailedDomainEvent(Guid UserId, int FailedAttemptCount) : DomainEvent;
