using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy.Events;

/// <summary>Raised when a tenant's mutable profile (name, branding, locale defaults, contact) changes.</summary>
/// <param name="TenantId">The affected tenant's id.</param>
public sealed record TenantUpdatedDomainEvent(Guid TenantId) : DomainEvent;
