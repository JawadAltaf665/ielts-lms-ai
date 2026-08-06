using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AgentRegistrationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AgentRegistration, AgentRegistrationId>(dbContext), IAgentRegistrationRepository
{
    public async Task<IReadOnlyList<AgentRegistration>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.AgentKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<AgentRegistration?> GetByKeyAsync(string agentKey, CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => a.AgentKey == agentKey && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
}
