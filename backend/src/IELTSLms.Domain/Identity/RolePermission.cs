using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
/// <summary>
/// Associative entity linking roles to permissions (ENT-007).
/// </summary>
public sealed class RolePermission : Entity<Guid>, ITenantScoped, IAuditableEntity
{
    private RolePermission(Guid id, RoleId roleId, PermissionId permissionId, Guid tenantId, DateTimeOffset createdAtUtc, Guid createdByUserId)
        : base(id)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        TenantId = tenantId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    private RolePermission() {}

    public RoleId RoleId { get; private init; }
    public PermissionId PermissionId { get; private init; }
    public Guid TenantId { get; private init; }
    
    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private init; }

    public static RolePermission Assign(RoleId roleId, PermissionId permissionId, Guid tenantId, DateTimeOffset nowUtc, Guid createdByUserId)
    {
        return new RolePermission(Guid.NewGuid(), roleId, permissionId, tenantId, nowUtc, createdByUserId);
    }
}
