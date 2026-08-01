using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>Aggregate access for <see cref="GrammarAttempt"/> (ERD ENT-016, mirrors ENT-023 ReadingAttempts).</summary>
public interface IGrammarAttemptRepository : IRepository<GrammarAttempt, GrammarAttemptId>
{
    /// <summary>A user's attempts, newest first — feeds the Sprint 9 cross-skill exam history.</summary>
    Task<IReadOnlyList<GrammarAttempt>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
