using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>Aggregate access for <see cref="GrammarAttempt"/> (ERD ENT-016, mirrors ENT-023 ReadingAttempts).</summary>
public interface IGrammarAttemptRepository : IRepository<GrammarAttempt, GrammarAttemptId>
{
}
