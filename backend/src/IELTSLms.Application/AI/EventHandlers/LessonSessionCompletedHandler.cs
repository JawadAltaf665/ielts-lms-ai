using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.AI.Events;
using IELTSLms.Domain.Learning;
using MediatR;

namespace IELTSLms.Application.AI.EventHandlers;

/// <summary>
/// Reacts to a completed AI Teacher lesson session (Phase 7 Sprint 4) the same way
/// <c>GrammarCompletedHandler</c> (Phase 6 Sprint 1) reacts to a scored Grammar attempt: reports the
/// activity to the shared, context-agnostic <see cref="IProgressTrackingService"/>. Unlike Grammar,
/// AI Teacher has no per-topic progress table of its own to update first — the sprint's business rule
/// is explicit that this is the only progress-update path, not a parallel one.
/// </summary>
public sealed class LessonSessionCompletedHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<LessonSessionCompletedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<LessonSessionCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        await progressTracking
            .RecordSkillActivityAsync(domainEvent.TenantId, domainEvent.UserId, LearningSkill.AiTeacher, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);
    }
}
