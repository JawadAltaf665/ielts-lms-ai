using IELTSLms.Domain.Certificates;

namespace IELTSLms.Abstractions.Persistence.Certificates;

/// <summary>Aggregate access for <see cref="Achievement"/> (ERD ENT-054).</summary>
public interface IAchievementRepository : IRepository<Achievement, AchievementId>
{
    /// <summary>Existence check backing idempotent awarding (ERD ENT-054: unique per (UserId, AchievementType) where single-award).</summary>
    Task<bool> ExistsAsync(Guid tenantId, Guid userId, string achievementType, CancellationToken cancellationToken = default);
}
