using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Conversations;

namespace IELTSLms.Application.AI.Tutor.Commands.StartTutorConversation;

/// <summary>Opens a new reactive Q&amp;A thread with AI Tutor (Phase 7 Sprint 4,
/// <c>FR-AITUTOR-001</c>). No AI call here — the conversation starts empty; the learner's first
/// question goes through <c>AskTutorCommand</c>. Entitlement-gated even though no gateway call happens
/// yet, so a non-entitled tenant cannot open a conversation it could never actually use.</summary>
public sealed record StartTutorConversationCommand(Guid TenantId, Guid UserId) : ICommand<ConversationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.tutor.use"];
}
