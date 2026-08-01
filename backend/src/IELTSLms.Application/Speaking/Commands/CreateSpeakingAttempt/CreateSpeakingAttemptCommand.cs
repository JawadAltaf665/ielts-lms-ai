using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;

/// <summary>
/// Creates a <see cref="Domain.Speaking.SpeakingAttempt"/> plus its backing <c>FileAsset</c> (ENT-057)
/// in one step — multipart upload, same exact pattern as <c>CreateListeningAudioCommand</c> (Sprint 5).
/// Learner-facing (<c>speaking.submit</c>): the attempt starts <see cref="Domain.Speaking.SpeakingAttemptStatus.Recorded"/>
/// and is not yet queued for evaluation — that's a separate step (<c>SubmitSpeakingAttemptCommand</c>),
/// per the sprint's two-endpoint split.
/// </summary>
public sealed record CreateSpeakingAttemptCommand(
    Guid TenantId, Guid UserId, Guid SpeakingTaskId, int DurationSeconds, Stream FileContent, string FileName,
    string ContentType, long SizeBytes) : ICommand<SpeakingAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.submit"];
}

public sealed record SpeakingAttemptResponse(
    Guid Id, Guid UserId, Guid SpeakingTaskId, int DurationSeconds, string? TranscriptText, string Status,
    DateTimeOffset? SubmittedAtUtc, Guid? SpeakingEvaluationId);
