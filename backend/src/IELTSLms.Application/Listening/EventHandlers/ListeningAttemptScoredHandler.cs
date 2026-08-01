using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Listening.Events;
using MediatR;

namespace IELTSLms.Application.Listening.EventHandlers;

/// <summary>
/// Reacts to a scored attempt: reports the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/> — same pattern <c>ReadingAttemptScoredHandler</c>
/// (Phase 6 Sprint 4) established.
/// </summary>
public sealed class ListeningAttemptScoredHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<ListeningAttemptScoredDomainEvent>>
{
    public Task Handle(DomainEventNotification<ListeningAttemptScoredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        return progressTracking.RecordSkillActivityAsync(
            domainEvent.TenantId, domainEvent.UserId, LearningSkill.Listening, clock.UtcNow, cancellationToken);
    }
}
