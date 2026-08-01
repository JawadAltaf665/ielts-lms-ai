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
}
