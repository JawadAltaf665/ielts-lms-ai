using IELTSLms.Domain.Reading;

namespace IELTSLms.Abstractions.Persistence.Reading;

/// <summary>Aggregate access for <see cref="ReadingAttempt"/> (ERD ENT-023).</summary>
public interface IReadingAttemptRepository : IRepository<ReadingAttempt, ReadingAttemptId>
{
    /// <summary>A user's submitted attempts, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<ReadingAttempt>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
