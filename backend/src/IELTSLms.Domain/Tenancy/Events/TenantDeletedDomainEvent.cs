using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when a tenant is soft-deleted (closure); data is retained per the retention policy.</summary>
/// <param name="TenantId">The closed tenant's id.</param>
public sealed record TenantDeletedDomainEvent(Guid TenantId) : DomainEvent;
