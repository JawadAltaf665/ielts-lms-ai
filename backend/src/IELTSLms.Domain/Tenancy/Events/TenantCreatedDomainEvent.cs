using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when a new tenant is provisioned in the active state.</summary>
/// <param name="TenantId">The newly created tenant's id.</param>
/// <param name="Slug">The tenant's globally unique slug.</param>
/// <param name="Code">The tenant's globally unique code.</param>
public sealed record TenantCreatedDomainEvent(Guid TenantId, string Slug, string Code) : DomainEvent;
