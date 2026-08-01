using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Speaking.Events;
using MediatR;

namespace IELTSLms.Application.Speaking.EventHandlers;

/// <summary>
/// Reacts to a completed evaluation: reports the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/> — same pattern as <see cref="Writing.EventHandlers.WritingEvaluatedHandler"/>.
/// Fires from <c>SpeakingEvaluationJob</c>'s own top-level save (not nested inside another dispatch).
/// </summary>
public sealed class SpeakingEvaluatedHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<SpeakingEvaluatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<SpeakingEvaluatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        return progressTracking.RecordSkillActivityAsync(
            domainEvent.TenantId, domainEvent.UserId, LearningSkill.Speaking, clock.UtcNow, cancellationToken);
    }
}
