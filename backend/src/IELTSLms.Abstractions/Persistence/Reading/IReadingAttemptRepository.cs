using IELTSLms.Domain.Reading;

namespace IELTSLms.Abstractions.Persistence.Reading;

/// <summary>Aggregate access for <see cref="ReadingAttempt"/> (ERD ENT-023).</summary>
public interface IReadingAttemptRepository : IRepository<ReadingAttempt, ReadingAttemptId>
{
}
