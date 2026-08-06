using IELTSLms.Domain.Learning;

namespace IELTSLms.Abstractions.Persistence.Learning;

/// <summary>Aggregate access for the canonical cross-module <see cref="ProgressTracking"/> (ERD ENT-055).</summary>
public interface IProgressTrackingRepository : IRepository<ProgressTracking, ProgressTrackingId>
{
    Task<ProgressTracking?> GetByUserAndSkillAsync(
        Guid tenantId, Guid userId, LearningSkill skill, CancellationToken cancellationToken = default);

    /// <summary>All per-skill rows for one learner — the Progress Analysis Agent's minimum-data-threshold
    /// input (Sprint 9).</summary>
    Task<IReadOnlyList<ProgressTracking>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Distinct user ids with any recorded activity since <paramref name="activeSinceUtc"/>,
    /// tenant-wide — the "active-learner cohort" the Sprint 9 daily workflow sweep iterates, without a
    /// dedicated Users read model. ponytail: a plain per-tenant scan; fine while learner counts stay in
    /// the thousands, revisit with a materialized cohort view if the sweep gets slow.</summary>
    Task<IReadOnlyList<Guid>> ListActiveUserIdsAsync(
        Guid tenantId, DateTimeOffset activeSinceUtc, CancellationToken cancellationToken = default);
}
