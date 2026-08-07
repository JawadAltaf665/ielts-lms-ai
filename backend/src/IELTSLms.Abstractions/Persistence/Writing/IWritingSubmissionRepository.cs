using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>Aggregate access for <see cref="WritingSubmission"/> (ERD ENT-028).</summary>
public interface IWritingSubmissionRepository : IRepository<WritingSubmission, WritingSubmissionId>
{
    /// <summary>A user's non-draft submissions, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<WritingSubmission>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Other learners' non-draft submissions for the same <see cref="WritingTask"/> within the tenant,
    /// excluding <paramref name="excludingSubmissionId"/> itself — the within-tenant comparison corpus
    /// for <c>ISimilarityCheckProvider</c> (Phase 7 Sprint 6). Scoped to the same task, not the whole
    /// tenant history: cross-task text overlap isn't a plagiarism/collusion signal.
    /// </summary>
    Task<IReadOnlyList<WritingSubmission>> ListSubmittedByTaskAsync(
        Guid tenantId, WritingTaskId taskId, WritingSubmissionId excludingSubmissionId, CancellationToken cancellationToken = default);
}
