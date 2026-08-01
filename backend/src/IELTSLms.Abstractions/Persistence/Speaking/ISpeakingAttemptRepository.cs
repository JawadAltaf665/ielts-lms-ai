using IELTSLms.Domain.Speaking;

namespace IELTSLms.Abstractions.Persistence.Speaking;

/// <summary>Aggregate access for <see cref="SpeakingAttempt"/> (ERD ENT-031).</summary>
public interface ISpeakingAttemptRepository : IRepository<SpeakingAttempt, SpeakingAttemptId>
{
}
