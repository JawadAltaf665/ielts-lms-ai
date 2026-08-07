using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class ConversationSummaryRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ConversationSummary, ConversationSummaryId>(dbContext), IConversationSummaryRepository
{
    public async Task<ConversationSummary?> GetLatestAsync(
        Guid tenantId, ConversationId conversationId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(s => s.TenantId == tenantId && s.ConversationId == conversationId)
            .OrderByDescending(s => s.CoversUpToSequence)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
}
