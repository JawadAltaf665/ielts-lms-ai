using IELTSLms.Domain.Speaking;

namespace IELTSLms.Abstractions.Persistence.Speaking;

/// <summary>Aggregate access for <see cref="SpeakingAttempt"/> (ERD ENT-031).</summary>
public interface ISpeakingAttemptRepository : IRepository<SpeakingAttempt, SpeakingAttemptId>
{
    /// <summary>A user's non-draft attempts, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<SpeakingAttempt>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
