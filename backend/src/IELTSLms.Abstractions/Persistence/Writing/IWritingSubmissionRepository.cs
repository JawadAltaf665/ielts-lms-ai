using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>Aggregate access for <see cref="WritingSubmission"/> (ERD ENT-028).</summary>
public interface IWritingSubmissionRepository : IRepository<WritingSubmission, WritingSubmissionId>
{
    /// <summary>A user's non-draft submissions, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<WritingSubmission>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
