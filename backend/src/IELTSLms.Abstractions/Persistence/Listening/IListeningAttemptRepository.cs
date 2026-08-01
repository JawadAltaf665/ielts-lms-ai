using IELTSLms.Domain.Listening;

namespace IELTSLms.Abstractions.Persistence.Listening;

/// <summary>Aggregate access for <see cref="ListeningAttempt"/> (ERD ENT-026).</summary>
public interface IListeningAttemptRepository : IRepository<ListeningAttempt, ListeningAttemptId>
{
    /// <summary>A user's submitted attempts, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<ListeningAttempt>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
