using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiResponseRepository : IAiResponseRepository
{
    private readonly List<AiResponse> _responses = [];

    public IReadOnlyList<AiResponse> Responses => _responses;

    public void Seed(AiResponse response) => _responses.Add(response);

    public Task<AiResponse?> GetByIdAsync(AiResponseId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_responses.FirstOrDefault(r => r.Id == id));

    public void Add(AiResponse aggregate) => _responses.Add(aggregate);

    public void Remove(AiResponse aggregate) => _responses.Remove(aggregate);
}
