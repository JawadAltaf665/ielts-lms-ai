using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

internal sealed class RolePermissionRepository(ApplicationDbContext dbContext) : IRolePermissionRepository
{
    public async Task<IReadOnlyList<string>> GetPermissionKeysForRoleNamesAsync(
        IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default)
    {
        if (roleNames.Count == 0)
        {
            return [];
        }

        var roleIds = dbContext.Set<Role>().Where(r => roleNames.Contains(r.Name)).Select(r => r.Id);

        // IgnoreQueryFilters: same reasoning as ExistsAsync below — TenantId on a RolePermission row
        // is provenance, not a scoping filter (no TenantId in the DB's own unique index). Without
        // this, EF's automatic tenant global filter restricts results to grants stamped with the
        // *caller's* tenant id, hiding every grant seeded under the system tenant (RbacCatalogSeeder)
        // for every other tenant — a real bug this exact query surfaced: an Institute Admin in a
        // freshly-provisioned tenant resolved zero permissions despite holding the role correctly.
        return await dbContext.Set<RolePermission>()
            .IgnoreQueryFilters()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(dbContext.Set<Permission>(), rp => rp.PermissionId, p => p.Id, (_, p) => p.Key)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(
        RoleId roleId, PermissionId permissionId, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the (RoleId, PermissionId) unique index has no TenantId column (see
        // interface remarks) — this check must see every grant regardless of the caller's ambient
        // tenant context (notably: there is none during startup seeding), or it will conclude a
        // grant is missing and attempt a duplicate insert that the database then rejects.
        await dbContext.Set<RolePermission>()
            .IgnoreQueryFilters()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken)
            .ConfigureAwait(false);

    public void Add(RolePermission rolePermission) => dbContext.Set<RolePermission>().Add(rolePermission);
}
