using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Learning;

internal sealed class LearningRecommendationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<LearningRecommendation, LearningRecommendationId>(dbContext), ILearningRecommendationRepository
{
    public async Task<LearningRecommendation?> GetActiveByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            r => r.TenantId == tenantId && r.UserId == userId && r.Status == LearningRecommendationStatus.Active,
            cancellationToken)
            .ConfigureAwait(false);

    public async Task<LearningRecommendation?> GetLatestDismissedAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set.Where(r => r.TenantId == tenantId && r.UserId == userId && r.Status == LearningRecommendationStatus.Dismissed)
            .OrderByDescending(r => r.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
}
