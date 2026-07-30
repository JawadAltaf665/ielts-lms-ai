using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AiOperationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AiOperation, AiOperationId>(dbContext), IAiOperationRepository
{
}
