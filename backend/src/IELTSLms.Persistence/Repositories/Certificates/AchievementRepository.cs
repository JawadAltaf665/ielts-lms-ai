using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Domain.Certificates;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Certificates;

internal sealed class AchievementRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Achievement, AchievementId>(dbContext), IAchievementRepository
{
    public async Task<bool> ExistsAsync(Guid tenantId, Guid userId, string achievementType, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            a => a.TenantId == tenantId && a.UserId == userId && a.AchievementType == achievementType, cancellationToken)
            .ConfigureAwait(false);
}
