using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Learning;

internal sealed class WeaknessProfileRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WeaknessProfile, WeaknessProfileId>(dbContext), IWeaknessProfileRepository
{
    public async Task<WeaknessProfile?> GetByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(w => w.TenantId == tenantId && w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
}
