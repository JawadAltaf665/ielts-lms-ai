using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="LessonSession"/> (Phase 7 Sprint 4).</summary>
public interface ILessonSessionRepository : IRepository<LessonSession, LessonSessionId>
{
    /// <summary>The caller's currently active (not <see cref="LessonStep.Completed"/>) session for this
    /// lesson, if any — backs the one-active-session-per-user-per-lesson invariant, mirroring
    /// <c>IMockExamRepository.GetActiveInProgressByUserAsync</c>.</summary>
    Task<LessonSession?> GetActiveByUserAndLessonAsync(
        Guid tenantId, Guid userId, Guid lessonId, CancellationToken cancellationToken = default);
}
