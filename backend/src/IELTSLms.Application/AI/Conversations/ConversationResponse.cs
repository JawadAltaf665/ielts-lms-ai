using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Conversations;

/// <summary>A learner's conversation thread (Phase 7 Sprint 3, ERD ENT-050B).</summary>
/// <param name="RowVersion">Optimistic-concurrency token, exposed for parity with other AI resources.</param>
public sealed record ConversationResponse(
    Guid Id,
    Guid UserId,
    string Feature,
    string Status,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset LastMessageAtUtc,
    uint RowVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

internal static class ConversationResponseMapper
{
    public static ConversationResponse ToResponse(Conversation conversation) => new(
        conversation.Id.Value,
        conversation.UserId,
        conversation.Feature,
        conversation.Status.ToString(),
        conversation.StartedAtUtc,
        conversation.LastMessageAtUtc,
        conversation.Version,
        conversation.CreatedAtUtc,
        conversation.UpdatedAtUtc);
}
