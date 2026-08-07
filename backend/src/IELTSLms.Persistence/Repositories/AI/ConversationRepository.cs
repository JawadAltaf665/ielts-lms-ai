using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class ConversationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Conversation, ConversationId>(dbContext), IConversationRepository
{
    public async Task<IReadOnlyList<Conversation>> ListByUserAsync(
        Guid tenantId, Guid userId, string? feature, ConversationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = Set.Where(c => c.TenantId == tenantId && c.UserId == userId);

        if (feature is not null)
        {
            query = query.Where(c => c.Feature == feature);
        }

        if (status is not null)
        {
            query = query.Where(c => c.Status == status);
        }

        return await query
            .OrderByDescending(c => c.LastMessageAtUtc).ThenByDescending(c => c.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
