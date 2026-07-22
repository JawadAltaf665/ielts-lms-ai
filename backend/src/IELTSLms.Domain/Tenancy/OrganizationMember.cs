using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// Associative entity linking users to organizations.
/// </summary>
public sealed class OrganizationMember : Entity<Guid>, ITenantScoped, IAuditableEntity
{
    private OrganizationMember(Guid id, Guid tenantId, OrganizationId organizationId, Guid userId, DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        OrganizationId = organizationId;
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
    }

    private OrganizationMember() {}

    public Guid TenantId { get; private init; }
    public OrganizationId OrganizationId { get; private init; }
    public Guid UserId { get; private init; }
    
    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static OrganizationMember Add(OrganizationId organizationId, Guid tenantId, Guid userId, DateTimeOffset nowUtc)
    {
        return new OrganizationMember(Guid.NewGuid(), tenantId, organizationId, userId, nowUtc);
    }
}
