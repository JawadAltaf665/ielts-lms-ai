using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="Conversation"/> (Phase 7 Sprint 3).</summary>
public interface IConversationRepository : IRepository<Conversation, ConversationId>
{
    /// <summary>
    /// All of a learner's conversations in this tenant, optionally filtered by feature/status, newest
    /// message first. ponytail: loads the full (naturally small, per-learner) set and lets the caller
    /// cursor-slice in memory — mirrors <c>GetExamHistoryQueryHandler</c>'s documented rationale; move to
    /// a server-side cursor predicate if a learner's conversation count stops being small.
    /// </summary>
    Task<IReadOnlyList<Conversation>> ListByUserAsync(
        Guid tenantId, Guid userId, string? feature, ConversationStatus? status, CancellationToken cancellationToken = default);
}
