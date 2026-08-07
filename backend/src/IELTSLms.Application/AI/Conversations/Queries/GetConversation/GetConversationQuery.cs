using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Conversations.Queries.GetConversation;

/// <summary>Gets one of the caller's own conversations plus one cursor page of its message history.
/// A conversation belonging to another user/tenant is <see cref="AiErrors.ConversationNotFound"/>
/// (404, not 403) — cross-tenant/cross-user existence is never confirmed.</summary>
public sealed record GetConversationQuery(Guid TenantId, Guid UserId, Guid ConversationId, string? Cursor, int Limit)
    : IQuery<ConversationDetailResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.conversation.view"];
}
