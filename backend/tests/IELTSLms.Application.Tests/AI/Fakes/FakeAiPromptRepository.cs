using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiPromptRepository : IAiPromptRepository
{
    private readonly List<AiPrompt> _prompts = [];

    public void Seed(AiPrompt prompt) => _prompts.Add(prompt);

    public Task<AiPrompt?> GetByIdAsync(AiPromptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_prompts.FirstOrDefault(p => p.Id == id));

    public void Add(AiPrompt aggregate) => _prompts.Add(aggregate);

    public void Remove(AiPrompt aggregate) => _prompts.Remove(aggregate);

    public Task<AiPrompt?> GetActiveForFeatureAsync(Guid tenantId, string feature, CancellationToken cancellationToken = default)
    {
        var tenantOverride = _prompts
            .Where(p => p.IsActive && p.Feature == feature && p.TenantId == tenantId)
            .MaxBy(p => p.PromptVersion);
        if (tenantOverride is not null)
        {
            return Task.FromResult<AiPrompt?>(tenantOverride);
        }

        var global = _prompts
            .Where(p => p.IsActive && p.Feature == feature && p.TenantId == null)
            .MaxBy(p => p.PromptVersion);
        return Task.FromResult(global);
    }
}
