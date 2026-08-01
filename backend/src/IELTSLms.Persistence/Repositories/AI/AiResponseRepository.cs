using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AiResponseRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AiResponse, AiResponseId>(dbContext), IAiResponseRepository
{
}
