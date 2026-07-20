using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when a suspended tenant is reinstated to the active state.</summary>
/// <param name="TenantId">The reactivated tenant's id.</param>
public sealed record TenantActivatedDomainEvent(Guid TenantId) : DomainEvent;
