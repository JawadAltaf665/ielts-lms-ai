using IELTSLms.Domain.Listening;

namespace IELTSLms.Abstractions.Persistence.Listening;

/// <summary>Aggregate access for <see cref="ListeningAttempt"/> (ERD ENT-026).</summary>
public interface IListeningAttemptRepository : IRepository<ListeningAttempt, ListeningAttemptId>
{
}
