using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when an active tenant is suspended (non-payment, policy violation, admin action).</summary>
/// <param name="TenantId">The suspended tenant's id.</param>
public sealed record TenantDeactivatedDomainEvent(Guid TenantId) : DomainEvent;
