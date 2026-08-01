using IELTSLms.Domain.Learning;

namespace IELTSLms.Abstractions.Persistence.Learning;

/// <summary>Aggregate access for the canonical cross-module <see cref="ProgressTracking"/> (ERD ENT-055).</summary>
public interface IProgressTrackingRepository : IRepository<ProgressTracking, ProgressTrackingId>
{
    Task<ProgressTracking?> GetByUserAndSkillAsync(
        Guid tenantId, Guid userId, LearningSkill skill, CancellationToken cancellationToken = default);
}
