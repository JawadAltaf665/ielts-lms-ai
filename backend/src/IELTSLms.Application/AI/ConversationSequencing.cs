using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI;

/// <summary>
/// Computes the next <see cref="ConversationMessage.Sequence"/> for a conversation (Phase 7 Sprint 4 —
/// the first callers that actually append messages, Sprint 3 only read them). Reuses
/// <see cref="IConversationMessageRepository.ListPageAsync"/>'s existing newest-first page rather than
/// adding a dedicated "max sequence" repository method for one call site's convenience.
/// </summary>
internal static class ConversationSequencing
{
    public static async Task<int> NextSequenceAsync(
        IConversationMessageRepository messages, Guid tenantId, ConversationId conversationId, CancellationToken cancellationToken)
    {
        var latest = await messages.ListPageAsync(tenantId, conversationId, beforeSequence: null, limit: 1, cancellationToken)
            .ConfigureAwait(false);
        return latest.Count == 0 ? 1 : latest[0].Sequence + 1;
    }
}
