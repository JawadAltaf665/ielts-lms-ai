using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.AI.Conversations.Queries.ListConversations;

/// <summary>Lists the caller's own conversations, cursor-paginated (API Design Standards §6.1 — a
/// high-churn, volatile collection), optionally filtered by feature/status.</summary>
public sealed record ListConversationsQuery(Guid TenantId, Guid UserId, string? Feature, string? Status, string? Cursor, int Limit)
    : IQuery<CursorPage<ConversationResponse>>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.conversation.view"];
}
