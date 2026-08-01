using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="AiResponse"/> (ERD ENT-052).</summary>
public interface IAiResponseRepository : IRepository<AiResponse, AiResponseId>
{
}
