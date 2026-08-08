using IELTSLms.Application.Identity;
using IELTSLms.Application.Identity.Authorization;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Persistence.Seeding;

/// <summary>
/// Seeds the built-in RBAC catalog (<see cref="RbacCatalog"/>) on startup: the seven global system
/// roles, the permission keys declared across the codebase, and the role→permission grants for the
/// system tenant (<see cref="IdentityDefaults.TenantId"/>) — the only tenant that exists until
/// host-based resolution (Multi-Tenant Architecture §5.1) brings real institute tenants online. New
/// tenants get their own grants from <c>CreateTenantCommandHandler</c>, which reuses the same
/// <see cref="RbacCatalog"/> data (Role/Permission rows are global and only need creating once).
/// Idempotent — safe to run on every cold start.
/// </summary>
internal sealed partial class RbacCatalogSeeder(
    IServiceScopeFactory scopeFactory, ILogger<RbacCatalogSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            var roleIdsByName = await EnsureRolesAsync(dbContext, now, cancellationToken).ConfigureAwait(false);
            var permissionIdsByKey = await EnsurePermissionsAsync(dbContext, now, cancellationToken).ConfigureAwait(false);
            var grantsAdded = await EnsureGrantsAsync(
                dbContext, roleIdsByName, permissionIdsByKey, now, cancellationToken).ConfigureAwait(false);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            RbacCatalogSeeded(logger, grantsAdded);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SeedingFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<Dictionary<string, RoleId>> EnsureRolesAsync(
        ApplicationDbContext dbContext, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<Role>()
            .Where(r => r.TenantId == null)
            .ToDictionaryAsync(r => r.Name, r => r.Id, cancellationToken)
            .ConfigureAwait(false);

        foreach (var definition in RbacCatalog.Roles)
        {
            if (existing.ContainsKey(definition.Name))
            {
                continue;
            }

            var role = Role.CreateSystemRole(RoleId.From(Guid.NewGuid()), definition.Name, definition.Description, now);
            dbContext.Set<Role>().Add(role);
            existing[definition.Name] = role.Id;
        }

        return existing;
    }

    private static async Task<Dictionary<string, PermissionId>> EnsurePermissionsAsync(
        ApplicationDbContext dbContext, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Set<Permission>()
            .ToDictionaryAsync(p => p.Key, p => p.Id, cancellationToken)
            .ConfigureAwait(false);

        foreach (var definition in RbacCatalog.Permissions)
        {
            if (existing.ContainsKey(definition.Key))
            {
                continue;
            }

            var permission = Permission.Create(
                PermissionId.From(Guid.NewGuid()),
                definition.Key,
                definition.Name,
                definition.Description,
                definition.Category,
                definition.Group,
                metadata: "{}",
                version: "1",
                now);
            dbContext.Set<Permission>().Add(permission);
            existing[definition.Key] = permission.Id;
        }

        return existing;
    }

    private static async Task<int> EnsureGrantsAsync(
        ApplicationDbContext dbContext,
        Dictionary<string, RoleId> roleIdsByName,
        Dictionary<string, PermissionId> permissionIdsByKey,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // TenantId on the row is provenance ("seeded under the system tenant"), not a scoping
        // dimension — the database's own unique index is (RoleId, PermissionId) alone (see
        // IRolePermissionRepository's remarks), so the idempotency check must be too. IgnoreQueryFilters
        // because there's no ambient tenant/HTTP context at startup for the automatic tenant filter to
        // match against — without it every existing grant would look invisible here, and the retry would
        // attempt a duplicate insert that the unique index then rejects.
        var tenantId = IdentityDefaults.TenantId;

        var existingGrants = await dbContext.Set<RolePermission>()
            .IgnoreQueryFilters()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingGrantKeys = existingGrants.Select(g => (g.RoleId, g.PermissionId)).ToHashSet();

        var added = 0;
        foreach (var (roleName, permissionKeys) in RbacCatalog.Grants)
        {
            if (!roleIdsByName.TryGetValue(roleName, out var roleId))
            {
                continue;
            }

            foreach (var permissionKey in permissionKeys)
            {
                if (!permissionIdsByKey.TryGetValue(permissionKey, out var permissionId))
                {
                    continue;
                }

                if (!existingGrantKeys.Add((roleId, permissionId)))
                {
                    continue;
                }

                dbContext.Set<RolePermission>().Add(
                    RolePermission.Assign(roleId, permissionId, tenantId, now, createdByUserId: Guid.Empty));
                added++;
            }
        }

        return added;
    }

    [LoggerMessage(EventId = 4100, Level = LogLevel.Information,
        Message = "RBAC catalog seeded ({GrantsAdded} new grants for the system tenant).")]
    private static partial void RbacCatalogSeeded(ILogger logger, int grantsAdded);

    [LoggerMessage(EventId = 4101, Level = LogLevel.Warning,
        Message = "Failed to seed the RBAC catalog; the database may be unavailable. Seeding will retry on next startup.")]
    private static partial void SeedingFailed(ILogger logger, Exception exception);
}
