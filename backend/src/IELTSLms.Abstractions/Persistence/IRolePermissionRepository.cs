using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Access to <see cref="RolePermission"/> grants (ENT-007). Not an <c>IRepository</c> — same
/// reasoning as <see cref="IUserRoleRepository"/>; callers work in terms of the permission keys a
/// set of roles resolves to (Auth §10 — permission-based authorization).
/// </summary>
/// <remarks>
/// A grant is keyed by <c>(RoleId, PermissionId)</c> alone — the database's own unique index
/// (<c>ix_role_permissions_role_permission</c>) has no <c>TenantId</c> column, so a given role
/// (global system role or a tenant's own custom role — either way its <c>RoleId</c> is already
/// unique) has exactly one grant per permission, not one copy per tenant. <c>TenantId</c> on the row
/// records which tenant context authored the grant (audit/provenance); it is not a second scoping
/// dimension to filter permission resolution by.
/// </remarks>
public interface IRolePermissionRepository
{
    /// <summary>
    /// The distinct permission keys granted to any role named in <paramref name="roleNames"/>. Used
    /// to resolve a caller's effective permissions from the role names already on their token
    /// (Auth §9.2, §10.1).
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionKeysForRoleNamesAsync(
        IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default);

    /// <summary>Whether this grant already exists, ignoring the soft/tenant query filters (seeding idempotency).</summary>
    Task<bool> ExistsAsync(RoleId roleId, PermissionId permissionId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new grant to be persisted on the next unit-of-work commit.</summary>
    void Add(RolePermission rolePermission);
}
