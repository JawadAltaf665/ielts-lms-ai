using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AiPromptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AiPrompt, AiPromptId>(dbContext), IAiPromptRepository
{
    public async Task<AiPrompt?> GetActiveForFeatureAsync(Guid tenantId, string feature, CancellationToken cancellationToken = default)
    {
        var tenantOverride = await Set
            .Where(p => p.IsActive && p.Feature == feature && p.TenantId == tenantId)
            .OrderByDescending(p => p.PromptVersion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenantOverride is not null)
        {
            return tenantOverride;
        }

        return await Set
            .Where(p => p.IsActive && p.Feature == feature && p.TenantId == null)
            .OrderByDescending(p => p.PromptVersion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AiPrompt>> GetVersionsAsync(
        Guid tenantId, string feature, string scope, CancellationToken cancellationToken = default) =>
        await Set
            .Where(p => p.TenantId == tenantId && p.Feature == feature && p.Scope == scope)
            .OrderByDescending(p => p.PromptVersion)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<AiPrompt>> ListAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, int skip, int take,
        CancellationToken cancellationToken = default) =>
        await Filter(tenantId, feature, scope, isActive)
            .OrderBy(p => p.Feature).ThenBy(p => p.Scope).ThenByDescending(p => p.PromptVersion)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<int> CountAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, CancellationToken cancellationToken = default) =>
        await Filter(tenantId, feature, scope, isActive).CountAsync(cancellationToken).ConfigureAwait(false);

    private IQueryable<AiPrompt> Filter(Guid tenantId, string? feature, string? scope, bool? isActive)
    {
        var query = Set.Where(p => p.TenantId == tenantId);
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
