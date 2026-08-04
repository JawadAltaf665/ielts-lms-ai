using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Abstractions.Persistence.SentenceBuilder;

/// <summary>Aggregate access for <see cref="SentenceAttempt"/> (ERD ENT-020).</summary>
public interface ISentenceAttemptRepository : IRepository<SentenceAttempt, SentenceAttemptId>
{
}
