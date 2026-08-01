using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Reading.Events;

/// <summary>
/// Raised when a learner's <see cref="ReadingAttempt"/> is scored (DDD §9.2, EVT-RDG-01). Drives the
/// cross-module canonical <c>ProgressTracking</c> (ERD ENT-055) through <c>IProgressTrackingService</c>
/// — the same pattern <c>GrammarCompletedDomainEvent</c> (Phase 6 Sprint 1) established.
/// </summary>
public sealed record ReadingAttemptScoredDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid ReadingPassageId,
    Guid ReadingAttemptId,
    int RawScore,
    int MaxScore,
    decimal Band) : DomainEvent;
