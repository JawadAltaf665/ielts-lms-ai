using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Teacher.Commands.StartLessonSession;

/// <summary>
/// Starts a structured AI Teacher session for a lesson (Phase 7 Sprint 4): creates the
/// <c>LessonSession</c> step-state machine plus its backing <c>Conversation</c>, and enqueues the
/// gateway call that produces the <c>Explain</c> step's opening content — the Teacher initiates, the
/// learner has said nothing yet (directive interaction shape).
/// <para>
/// <paramref name="Topic"/> stands in for a real lesson-content lookup: the Course/Module/Lesson
/// content model (ERD ENT-014) does not exist yet in this codebase, so there is nothing server-side to
/// explain from. See <see cref="Domain.AI.LessonSession"/>'s remarks and this sprint's reconciliation
/// notes for the follow-up once Lessons ships.
/// </para>
/// </summary>
public sealed record StartLessonSessionCommand(Guid TenantId, Guid UserId, Guid LessonId, string Topic)
    : ICommand<LessonSessionAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.teacher.use"];
}

public sealed record LessonSessionAcceptedResponse(Guid LessonSessionId, Guid OperationId, string Status);
