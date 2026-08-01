using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Reading.Events;
using MediatR;

namespace IELTSLms.Application.Reading.EventHandlers;

/// <summary>
/// Reacts to a scored attempt: reports the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/> — Reading has no per-passage progress table of its own
/// (unlike Grammar/Vocabulary), so this is the sole reaction (Sprint 4 acceptance criteria: "Reuses
/// Sprint 1's IProgressTrackingService — no parallel progress table").
/// </summary>
public sealed class ReadingAttemptScoredHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<ReadingAttemptScoredDomainEvent>>
{
    public Task Handle(DomainEventNotification<ReadingAttemptScoredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        return progressTracking.RecordSkillActivityAsync(domainEvent.TenantId, domainEvent.UserId, LearningSkill.Reading, clock.UtcNow, cancellationToken);
    }
}
