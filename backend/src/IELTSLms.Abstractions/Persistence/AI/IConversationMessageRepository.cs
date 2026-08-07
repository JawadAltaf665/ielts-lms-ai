using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="ConversationMessage"/> (Phase 7 Sprint 3).</summary>
public interface IConversationMessageRepository : IRepository<ConversationMessage, ConversationMessageId>
{
    /// <summary>Messages after <paramref name="afterSequence"/>, ascending — the context-assembly tail
    /// since the latest <see cref="ConversationSummary"/> (or the conversation start if none exists).</summary>
    Task<IReadOnlyList<ConversationMessage>> ListSinceSequenceAsync(
        Guid tenantId, ConversationId conversationId, int afterSequence, CancellationToken cancellationToken = default);

    /// <summary>One cursor page of message history, newest first, for the message-history read surface.
    /// <paramref name="beforeSequence"/> is the exclusive upper bound (<see langword="null"/> for the
    /// first page); returns up to <paramref name="limit"/> + 1 rows so the caller can detect <c>HasMore</c>.</summary>
    Task<IReadOnlyList<ConversationMessage>> ListPageAsync(
        Guid tenantId, ConversationId conversationId, int? beforeSequence, int limit, CancellationToken cancellationToken = default);
}
