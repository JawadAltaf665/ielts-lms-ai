using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarAttempt, GrammarAttemptId>(dbContext), IGrammarAttemptRepository
{
}
