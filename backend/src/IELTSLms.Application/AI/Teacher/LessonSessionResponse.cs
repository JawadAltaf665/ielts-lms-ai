using IELTSLms.Application.AI.Conversations;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Teacher;

/// <summary>A learner's AI Teacher session for one lesson (Phase 7 Sprint 4).</summary>
public sealed record LessonSessionResponse(
    Guid Id,
    Guid UserId,
    Guid LessonId,
    Guid ConversationId,
    string CurrentStep,
    int CheckAttempts,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    uint RowVersion);

/// <summary><see cref="LessonSessionResponse"/> plus its conversation history — the "current step,
/// history" shape the sprint file's <c>GET</c> endpoint specifies.</summary>
public sealed record LessonSessionDetailResponse(
    LessonSessionResponse Session, IReadOnlyList<ConversationMessageResponse> Messages);

internal static class LessonSessionResponseMapper
{
    public static LessonSessionResponse ToResponse(LessonSession session) => new(
        session.Id.Value,
        session.UserId,
        session.LessonId,
        session.ConversationId.Value,
        session.CurrentStep.ToString(),
        session.CheckAttempts,
        session.StartedAtUtc,
        session.CompletedAtUtc,
        session.Version);
}
