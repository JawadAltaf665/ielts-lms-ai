using IELTSLms.Domain.Speaking;

namespace IELTSLms.Abstractions.Persistence.Speaking;

/// <summary>Aggregate access for <see cref="SpeakingEvaluation"/> (ERD ENT-032).</summary>
public interface ISpeakingEvaluationRepository : IRepository<SpeakingEvaluation, SpeakingEvaluationId>
{
    /// <summary>The single active evaluation for an attempt, or <see langword="null"/> if not yet evaluated (ERD's 1:1 constraint).</summary>
    Task<SpeakingEvaluation?> GetByAttemptIdAsync(SpeakingAttemptId attemptId, CancellationToken cancellationToken = default);
}
