using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiProviderRepository : IAiProviderRepository
{
    private readonly List<AiProvider> _providers = [];

    public void Seed(AiProvider provider) => _providers.Add(provider);

    public Task<AiProvider?> GetByIdAsync(AiProviderId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_providers.FirstOrDefault(p => p.Id == id));

    public void Add(AiProvider aggregate) => _providers.Add(aggregate);

    public void Remove(AiProvider aggregate) => _providers.Remove(aggregate);

    public Task<AiProvider?> GetActiveByKindAsync(AiProviderKind kind, CancellationToken cancellationToken = default) =>
        Task.FromResult(_providers.FirstOrDefault(p => p.Kind == kind && p.IsActive));
}
