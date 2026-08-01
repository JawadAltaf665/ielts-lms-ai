using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Conversations;

/// <summary>One turn in a conversation's message history (Phase 7 Sprint 3, ERD ENT-050C).</summary>
public sealed record ConversationMessageResponse(
    Guid Id,
    string Role,
    string Content,
    int Sequence,
    int TokenCount,
    Guid? AiResponseId,
    DateTimeOffset CreatedAtUtc);

internal static class ConversationMessageResponseMapper
{
    public static ConversationMessageResponse ToResponse(ConversationMessage message) => new(
        message.Id.Value,
        message.Role.ToString(),
        message.Content,
        message.Sequence,
        message.TokenCount,
        message.AiResponseId?.Value,
        message.CreatedAtUtc);
}
