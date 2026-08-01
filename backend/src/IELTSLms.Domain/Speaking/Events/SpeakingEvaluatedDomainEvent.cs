using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Speaking.Events;

/// <summary>
/// Raised when a <see cref="SpeakingEvaluation"/> is attached to its <see cref="SpeakingAttempt"/>.
/// Drives <c>IProgressTrackingService</c>, same pattern as <see cref="Writing.Events.WritingEvaluatedDomainEvent"/>
/// — raised from <c>SpeakingEvaluationJob</c>'s own top-level save, not nested inside another dispatch.
/// </summary>
public sealed record SpeakingEvaluatedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid SpeakingTaskId,
    Guid SpeakingAttemptId,
    Guid SpeakingEvaluationId,
    decimal EstimatedBand) : DomainEvent;
