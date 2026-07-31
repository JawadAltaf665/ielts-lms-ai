using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Listening.Events;

/// <summary>
/// Raised when a learner's <see cref="ListeningAttempt"/> is scored (DDD §13.11, EVT-LSN-01). Drives
/// the cross-module canonical <c>ProgressTracking</c> (ERD ENT-055) through
/// <c>IProgressTrackingService</c> — the same pattern <c>ReadingAttemptScoredDomainEvent</c>
/// (Phase 6 Sprint 4) established.
/// </summary>
public sealed record ListeningAttemptScoredDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid ListeningAudioId,
    Guid ListeningAttemptId,
    int RawScore,
    int MaxScore,
    decimal Band) : DomainEvent;
