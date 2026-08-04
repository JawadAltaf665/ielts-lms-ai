using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Persistence.Repositories.SentenceBuilder;

internal sealed class SentenceAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SentenceAttempt, SentenceAttemptId>(dbContext), ISentenceAttemptRepository
{
}
