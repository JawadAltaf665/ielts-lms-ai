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

    public Task<AiPrompt?> GetActiveForFeatureAndScopeAsync(
        Guid tenantId, string feature, string scope, CancellationToken cancellationToken = default)
    {
        var tenantOverride = _prompts
            .Where(p => p.IsActive && p.Feature == feature && p.Scope == scope && p.TenantId == tenantId)
            .MaxBy(p => p.PromptVersion);
        if (tenantOverride is not null)
        {
            return Task.FromResult<AiPrompt?>(tenantOverride);
        }

        var global = _prompts
            .Where(p => p.IsActive && p.Feature == feature && p.Scope == scope && p.TenantId == null)
            .MaxBy(p => p.PromptVersion);
        return Task.FromResult(global);
    }

    public Task<IReadOnlyList<AiPrompt>> GetVersionsAsync(
        Guid tenantId, string feature, string scope, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AiPrompt> versions = _prompts
            .Where(p => p.TenantId == tenantId && p.Feature == feature && p.Scope == scope)
            .OrderByDescending(p => p.PromptVersion)
            .ToList();
        return Task.FromResult(versions);
    }

    public Task<IReadOnlyList<AiPrompt>> ListAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AiPrompt> items = Filter(tenantId, feature, scope, isActive)
            .OrderBy(p => p.Feature).ThenBy(p => p.Scope).ThenByDescending(p => p.PromptVersion)
            .Skip(skip).Take(take)
            .ToList();
        return Task.FromResult(items);
    }

    public Task<int> CountAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, CancellationToken cancellationToken = default) =>
        Task.FromResult(Filter(tenantId, feature, scope, isActive).Count());

    private IEnumerable<AiPrompt> Filter(Guid tenantId, string? feature, string? scope, bool? isActive)
    {
        var query = _prompts.Where(p => p.TenantId == tenantId);
        if (feature is not null)
        {
            query = query.Where(p => p.Feature == feature);
        }

        if (scope is not null)
        {
            query = query.Where(p => p.Scope == scope);
        }

        if (isActive is not null)
        {
            query = query.Where(p => p.IsActive == isActive);
        }

        return query;
    }
}
