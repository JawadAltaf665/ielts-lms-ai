using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Learning;

internal sealed class ProgressTrackingRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ProgressTracking, ProgressTrackingId>(dbContext), IProgressTrackingRepository
{
    public async Task<ProgressTracking?> GetByUserAndSkillAsync(
        Guid tenantId, Guid userId, LearningSkill skill, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.UserId == userId && p.Skill == skill, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<ProgressTracking>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.TenantId == tenantId && p.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Guid>> ListActiveUserIdsAsync(
        Guid tenantId, DateTimeOffset activeSinceUtc, CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.TenantId == tenantId && p.LastActivityAtUtc >= activeSinceUtc)
            .Select(p => p.UserId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
