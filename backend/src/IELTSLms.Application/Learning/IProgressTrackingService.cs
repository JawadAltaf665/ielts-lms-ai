using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Learning;

/// <summary>
/// The single write path into the canonical cross-module <c>ProgressTracking</c> (ERD ENT-055, DDD
/// §13.4 Learning Management). Every skill module (Grammar first; Vocabulary/Reading/Listening/
/// Writing/Speaking later) calls this from its own <c>&lt;Skill&gt;Completed</c> domain-event handler
/// — never a direct cross-context call from the submission handler itself (DDD §12.2 DEP-2).
/// </summary>
public interface IProgressTrackingService
{
    /// <summary>Records one completed skill activity for a learner, creating the row on first activity.</summary>
    Task RecordSkillActivityAsync(
        Guid tenantId, Guid userId, LearningSkill skill, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}
