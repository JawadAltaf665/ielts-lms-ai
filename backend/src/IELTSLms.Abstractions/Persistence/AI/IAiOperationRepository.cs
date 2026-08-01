using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="AiOperation"/>, the async operation-resource backing store.</summary>
public interface IAiOperationRepository : IRepository<AiOperation, AiOperationId>
{
}
