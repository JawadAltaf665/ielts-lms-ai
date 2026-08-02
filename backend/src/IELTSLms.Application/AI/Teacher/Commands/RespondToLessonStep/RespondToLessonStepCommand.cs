using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.Teacher.Commands.RespondToLessonStep;

/// <summary>The learner's turn within a structured AI Teacher session (Phase 7 Sprint 4): the AI call
/// this enqueues both produces the current step's feedback/content and — for the
/// <c>check-understanding</c> step only — decides whether the session advances (business rule: a
/// failed check never advances on its own).</summary>
public sealed record RespondToLessonStepCommand(Guid TenantId, Guid UserId, Guid LessonSessionId, string Message)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.teacher.use"];
}
