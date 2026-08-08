using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

internal sealed class UserRoleRepository(ApplicationDbContext dbContext) : IUserRoleRepository
{
    public async Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.Set<UserRole>()
            .Where(ur => ur.UserId == UserId.From(userId) && ur.TenantId == tenantId)
            .Join(dbContext.Set<Role>(), ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(
        Guid userId, RoleId roleId, Guid tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.Set<UserRole>()
            .AnyAsync(
                ur => ur.UserId == UserId.From(userId) && ur.RoleId == roleId && ur.TenantId == tenantId,
                cancellationToken)
            .ConfigureAwait(false);

    public void Add(UserRole userRole) => dbContext.Set<UserRole>().Add(userRole);
}
