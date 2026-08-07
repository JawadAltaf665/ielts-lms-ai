using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Teacher.Queries.GetLessonSession;

/// <summary>Gets one of the caller's own lesson sessions: current step, attempt count, and its full
/// conversation history (bounded — a lesson session is four short steps, not an open-ended thread, so
/// unlike <c>GetConversationQuery</c> this doesn't need cursor pagination).
/// <para>
/// Self-scoped only for now, mirroring <c>GetConversationQuery</c> — the sprint file's cohort-teacher
/// view (<c>analytics.view.cohort</c>) needs a teacher/cohort membership lookup this sprint doesn't
/// build; flagged in Reconciliation Notes rather than stubbed here.
/// </para>
/// </summary>
public sealed record GetLessonSessionQuery(Guid TenantId, Guid UserId, Guid LessonSessionId)
    : IQuery<LessonSessionDetailResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.teacher.use"];
}
