using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.AI.Conversations;

/// <summary>A conversation plus one cursor page of its message history (Phase 7 Sprint 3).</summary>
public sealed record ConversationDetailResponse(ConversationResponse Conversation, CursorPage<ConversationMessageResponse> Messages);
