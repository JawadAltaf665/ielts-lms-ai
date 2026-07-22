using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// Associative entity linking users to roles (ENT-008).
/// </summary>
public sealed class UserRole : Entity<Guid>, ITenantScoped, IAuditableEntity
{
    private UserRole(Guid id, UserId userId, RoleId roleId, Guid tenantId, DateTimeOffset createdAtUtc, Guid createdByUserId)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    private UserRole() {}

    public UserId UserId { get; private init; }
    public RoleId RoleId { get; private init; }
    public Guid TenantId { get; private init; }
    
    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private init; }

    public static UserRole Assign(UserId userId, RoleId roleId, Guid tenantId, DateTimeOffset nowUtc, Guid createdByUserId)
    {
        return new UserRole(Guid.NewGuid(), userId, roleId, tenantId, nowUtc, createdByUserId);
    }
}
