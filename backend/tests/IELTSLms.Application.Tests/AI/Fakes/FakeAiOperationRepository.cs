using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiOperationRepository : IAiOperationRepository
{
    private readonly List<AiOperation> _operations = [];

    public IReadOnlyList<AiOperation> Operations => _operations;

    public void Seed(AiOperation operation) => _operations.Add(operation);

    public Task<AiOperation?> GetByIdAsync(AiOperationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_operations.FirstOrDefault(o => o.Id == id));

    public void Add(AiOperation aggregate) => _operations.Add(aggregate);

    public void Remove(AiOperation aggregate) => _operations.Remove(aggregate);
}
