using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class ConversationMessageRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ConversationMessage, ConversationMessageId>(dbContext), IConversationMessageRepository
{
    public async Task<IReadOnlyList<ConversationMessage>> ListSinceSequenceAsync(
        Guid tenantId, ConversationId conversationId, int afterSequence, CancellationToken cancellationToken = default) =>
        await Set
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && m.Sequence > afterSequence)
            .OrderBy(m => m.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<ConversationMessage>> ListPageAsync(
        Guid tenantId, ConversationId conversationId, int? beforeSequence, int limit, CancellationToken cancellationToken = default)
    {
        var query = Set.Where(m => m.TenantId == tenantId && m.ConversationId == conversationId);

        if (beforeSequence is { } sequence)
        {
            query = query.Where(m => m.Sequence < sequence);
        }

        return await query
            .OrderByDescending(m => m.Sequence)
            .Take(limit + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
