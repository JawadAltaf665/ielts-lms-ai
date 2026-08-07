using IELTSLms.Domain.Speaking;

namespace IELTSLms.Abstractions.Persistence.Speaking;

/// <summary>Aggregate access for <see cref="SpeakingEvaluation"/> (ERD ENT-032).</summary>
public interface ISpeakingEvaluationRepository : IRepository<SpeakingEvaluation, SpeakingEvaluationId>
{
    /// <summary>The single active evaluation for an attempt, or <see langword="null"/> if not yet evaluated (ERD's 1:1 constraint).</summary>
    Task<SpeakingEvaluation?> GetByAttemptIdAsync(SpeakingAttemptId attemptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// All evaluations in the tenant still awaiting human review (flagged, not yet reviewed or
    /// overridden), newest first — the review queue's single source query. Cursor-windowed in the query
    /// handler, same in-memory-page pattern as <c>ConversationRepository.ListByUserAsync</c>.
    /// </summary>
    Task<IReadOnlyList<SpeakingEvaluation>> ListPendingReviewAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
