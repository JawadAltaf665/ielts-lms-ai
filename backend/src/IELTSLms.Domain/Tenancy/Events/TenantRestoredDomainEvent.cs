using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when a soft-deleted tenant is restored within its retention window.</summary>
/// <param name="TenantId">The restored tenant's id.</param>
public sealed record TenantRestoredDomainEvent(Guid TenantId) : DomainEvent;
