using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AiProviderRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AiProvider, AiProviderId>(dbContext), IAiProviderRepository
{
    public async Task<AiProvider?> GetActiveByKindAsync(AiProviderKind kind, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(p => p.Kind == kind && p.IsActive, cancellationToken).ConfigureAwait(false);
}
