using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity.Events;

/// <summary>Raised when a new user account is registered and awaiting email verification.</summary>
/// <param name="UserId">The newly created user's id.</param>
/// <param name="TenantId">The tenant the user belongs to (column only until Sprint 3 wires tenancy).</param>
/// <param name="Email">The registered email address.</param>
public sealed record UserRegisteredDomainEvent(Guid UserId, Guid TenantId, string Email) : DomainEvent;
