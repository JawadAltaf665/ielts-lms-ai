using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Tenancy;

public enum OrganizationStatus
{
    Active = 0,
    Suspended = 1,
    Closed = 2
}

/// <summary>
/// Commercial entity mapped to a tenant; may contain sub-units (ENT-002).
/// </summary>
public sealed class Organization : AggregateRoot<OrganizationId>, IAuditableEntity, ITenantScoped, ISoftDeletable
{
    private Organization(OrganizationId id, Guid tenantId, string name, OrganizationId? parentOrganizationId, DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        ParentOrganizationId = parentOrganizationId;
        Status = OrganizationStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Organization()
    {
        Name = null!;
    }

    public Guid TenantId { get; private init; }
    public string Name { get; private set; }
    public OrganizationId? ParentOrganizationId { get; private set; }
    public OrganizationStatus Status { get; private set; }
    
    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    
    public uint Version { get; private set; }

    public static Organization Create(OrganizationId id, Guid tenantId, string name, OrganizationId? parentOrganizationId, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Organization(id, tenantId, name, parentOrganizationId, nowUtc);
    }

    public void UpdateName(string name, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        UpdatedAtUtc = nowUtc;
    }

    public void Suspend(DateTimeOffset nowUtc)
    {
        if (IsDeleted || Status == OrganizationStatus.Closed)
        {
            return;
        }
        Status = OrganizationStatus.Suspended;
        UpdatedAtUtc = nowUtc;
    }

    public void Reactivate(DateTimeOffset nowUtc)
    {
        if (IsDeleted || Status == OrganizationStatus.Closed)
        {
            return;
        }
        Status = OrganizationStatus.Active;
        UpdatedAtUtc = nowUtc;
    }

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }
        IsDeleted = true;
        Status = OrganizationStatus.Closed;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
