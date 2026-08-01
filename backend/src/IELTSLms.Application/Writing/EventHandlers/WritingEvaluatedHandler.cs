using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Writing.Events;
using MediatR;

namespace IELTSLms.Application.Writing.EventHandlers;

/// <summary>
/// Reacts to a completed evaluation: reports the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/> (POL-04) — same pattern
/// <see cref="Reading.EventHandlers.ReadingAttemptScoredHandler"/>/<see cref="Listening.EventHandlers.ListeningAttemptScoredHandler"/>
/// established. Fires from <c>WritingEvaluationJob</c>'s own top-level save (not nested inside another
/// dispatch), so no reentrancy concerns apply here — see <see cref="WritingSubmittedDomainEvent"/>'s
/// remarks for why that event, unlike this one, has no handler this sprint.
/// </summary>
public sealed class WritingEvaluatedHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<WritingEvaluatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<WritingEvaluatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        return progressTracking.RecordSkillActivityAsync(
            domainEvent.TenantId, domainEvent.UserId, LearningSkill.Writing, clock.UtcNow, cancellationToken);
    }
}
