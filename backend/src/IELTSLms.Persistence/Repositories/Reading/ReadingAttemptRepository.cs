using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Persistence.Repositories.Reading;

internal sealed class ReadingAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ReadingAttempt, ReadingAttemptId>(dbContext), IReadingAttemptRepository
{
}
