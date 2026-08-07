using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>Aggregate access for <see cref="WritingEvaluation"/> (ERD ENT-029).</summary>
public interface IWritingEvaluationRepository : IRepository<WritingEvaluation, WritingEvaluationId>
{
    /// <summary>The single active evaluation for a submission, or <see langword="null"/> if not yet evaluated (ERD's 1:1 constraint).</summary>
    Task<WritingEvaluation?> GetBySubmissionIdAsync(WritingSubmissionId submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// All evaluations in the tenant still awaiting human review (flagged, not yet reviewed or
    /// overridden), newest first — the review queue's single source query. Cursor-windowed in the query
    /// handler. Mirrors <c>ISpeakingEvaluationRepository.ListPendingReviewAsync</c>.
    /// </summary>
    Task<IReadOnlyList<WritingEvaluation>> ListPendingReviewAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
