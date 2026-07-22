using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A named set of permissions (ENT-005). Can be a system role (global) or a custom role (tenant-scoped).
/// </summary>
public sealed class Role : AggregateRoot<RoleId>, IAuditableEntity, ISoftDeletable
{
    private Role(RoleId id, Guid? tenantId, string name, string description, bool isSystemRole, DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
        CreatedAtUtc = createdAtUtc;
    }

    private Role()
    {
        Name = null!;
        Description = null!;
    }

    /// <summary>
    /// The owning tenant, if this is a custom role. Null if this is a global system role.
    /// </summary>
    public Guid? TenantId { get; private init; }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystemRole { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public uint Version { get; private set; }

    public static Role CreateSystemRole(RoleId id, string name, string description, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Role(id, null, name, description, isSystemRole: true, nowUtc);
    }

    public static Role CreateCustomRole(RoleId id, Guid tenantId, string name, string description, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Role(id, tenantId, name, description, isSystemRole: false, nowUtc);
    }

    public void Update(string name, string description, DateTimeOffset nowUtc)
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be modified.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        Name = name;
        Description = description;
        UpdatedAtUtc = nowUtc;
    }

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be deleted.");
        }

        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
