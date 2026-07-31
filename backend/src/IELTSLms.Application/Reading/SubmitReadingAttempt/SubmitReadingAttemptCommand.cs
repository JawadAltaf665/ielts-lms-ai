using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Reading.SubmitReadingAttempt;

/// <summary>
/// Submits answers for synchronous, rule-based grading. Rejected if past the server-computed deadline
/// (<c>DeadlineAt</c> stamped by <c>StartReadingAttempt</c>); resubmitting an already-scored attempt is
/// idempotent — it returns the stored result rather than rescoring (ERD §4.3).
/// </summary>
public sealed record SubmitReadingAttemptCommand(Guid TenantId, Guid UserId, Guid AttemptId, IReadOnlyList<ReadingAnswerSubmission> Answers)
    : ICommand<ReadingAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.attempt"];
}

public sealed record ReadingAnswerSubmission(Guid QuestionId, string? Answer);

public sealed record ReadingAttemptResponse(
    Guid Id, Guid PassageId, DateTimeOffset StartedAtUtc, DateTimeOffset DeadlineAtUtc, DateTimeOffset? SubmittedAtUtc,
    int? RawScore, int? MaxScore, decimal? Band, IReadOnlyList<ReadingAttemptAnswerResponse>? Answers);

public sealed record ReadingAttemptAnswerResponse(Guid QuestionId, string? SubmittedAnswer, bool IsCorrect);
