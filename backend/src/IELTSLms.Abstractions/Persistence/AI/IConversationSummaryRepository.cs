using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="ConversationSummary"/> (Phase 7 Sprint 3).</summary>
public interface IConversationSummaryRepository : IRepository<ConversationSummary, ConversationSummaryId>
{
    /// <summary>The most recent summary for a conversation (highest <c>CoversUpToSequence</c>), or
    /// <see langword="null"/> if the conversation has never been summarized.</summary>
    Task<ConversationSummary?> GetLatestAsync(Guid tenantId, ConversationId conversationId, CancellationToken cancellationToken = default);
}
