using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Persistence.Repositories.Speaking;

internal sealed class SpeakingAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SpeakingAttempt, SpeakingAttemptId>(dbContext), ISpeakingAttemptRepository
{
}
