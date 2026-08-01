using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Listening.SubmitListeningAttempt;

/// <summary>
/// Submits answers for synchronous, rule-based grading — rejected once the server-computed deadline
/// has passed; resubmitting an already-scored attempt is idempotent (ERD §4.3), same pattern as
/// <c>SubmitReadingAttemptCommand</c> (Phase 6 Sprint 4).
/// </summary>
public sealed record SubmitListeningAttemptCommand(Guid TenantId, Guid UserId, Guid AttemptId, IReadOnlyList<ListeningAnswerSubmission> Answers)
    : ICommand<ListeningAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.attempt"];
}

public sealed record ListeningAnswerSubmission(Guid QuestionId, string? Answer);

public sealed record ListeningAttemptResponse(
    Guid Id, Guid AudioId, string Mode, DateTimeOffset StartedAtUtc, DateTimeOffset DeadlineAtUtc, DateTimeOffset? SubmittedAtUtc,
    int? RawScore, int? MaxScore, decimal? Band, int ReplayCount, IReadOnlyList<ListeningAttemptAnswerResponse>? Answers);

public sealed record ListeningAttemptAnswerResponse(Guid QuestionId, string? SubmittedAnswer, bool IsCorrect);
