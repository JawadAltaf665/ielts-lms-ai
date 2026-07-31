using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Persistence.Repositories.Listening;

internal sealed class ListeningAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ListeningAttempt, ListeningAttemptId>(dbContext), IListeningAttemptRepository
{
}
