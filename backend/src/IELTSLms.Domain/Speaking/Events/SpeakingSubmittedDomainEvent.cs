using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Speaking.Events;

/// <summary>
/// Raised when a <see cref="SpeakingAttempt"/> is submitted (EVT-SPK-01). No handler subscribes to
/// this — same reasoning as <see cref="Writing.Events.WritingSubmittedDomainEvent"/>: the AI-evaluation
/// kickoff (transcription + scoring) is performed directly inside
/// <c>SubmitSpeakingAttemptCommandHandler</c>, in the same transaction that creates the
/// <c>AiOperation</c>, so the 202 response can return that operation's id without a reentrant nested
/// <c>SaveChangesAsync</c> from post-commit event dispatch. Still raised for audit/future-subscriber
/// parity with the DDD event catalog.
/// </summary>
public sealed record SpeakingSubmittedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid SpeakingTaskId,
    Guid SpeakingAttemptId) : DomainEvent;
