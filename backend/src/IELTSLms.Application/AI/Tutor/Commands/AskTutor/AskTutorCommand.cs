using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.Tutor.Commands.AskTutor;

/// <summary>One reactive turn in an AI Tutor conversation (Phase 7 Sprint 4, <c>FR-AITUTOR-001</c>).
/// Entitlement- and quota-gated before enqueueing (INV-03), exactly like Grammar Coach; a
/// <see cref="Tutor.TutorScopeGuard"/> check happens first inside the handler and can short-circuit
/// the whole thing without ever touching quota or the gateway.</summary>
public sealed record AskTutorCommand(Guid TenantId, Guid UserId, Guid ConversationId, string Question)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.tutor.use"];
}
