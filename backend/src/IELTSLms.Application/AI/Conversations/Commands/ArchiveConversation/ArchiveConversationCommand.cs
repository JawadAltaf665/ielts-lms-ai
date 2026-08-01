using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Conversations.Commands.ArchiveConversation;

/// <summary>Archives (soft-deletes) one of the caller's own conversations — learner-initiated cleanup,
/// not a hard purge (retention policy still applies). Idempotent.</summary>
public sealed record ArchiveConversationCommand(Guid TenantId, Guid UserId, Guid ConversationId) : ICommand, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.conversation.view"];
}
