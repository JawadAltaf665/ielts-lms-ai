using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Writing.Events;

/// <summary>
/// Raised when a <see cref="WritingEvaluation"/> is attached to its <see cref="WritingSubmission"/>
/// (DDD §9.2, EVT-AI-01). Drives <c>IProgressTrackingService</c> (POL-04), the same pattern
/// <see cref="Reading.Events.ReadingAttemptScoredDomainEvent"/>/<see cref="Listening.Events.ListeningAttemptScoredDomainEvent"/>
/// established — raised from <c>WritingEvaluationJob</c>'s own top-level save, not nested inside
/// another dispatch, so the standard post-commit handler wiring applies unmodified.
/// </summary>
public sealed record WritingEvaluatedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid WritingTaskId,
    Guid WritingSubmissionId,
    Guid WritingEvaluationId,
    decimal EstimatedBand) : DomainEvent;
