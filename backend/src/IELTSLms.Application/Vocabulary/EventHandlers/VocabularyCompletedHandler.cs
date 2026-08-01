using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Vocabulary.Events;
using MediatR;

namespace IELTSLms.Application.Vocabulary.EventHandlers;

/// <summary>
/// Reacts to a graded review by reporting the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/> — the pattern <c>GrammarCompletedHandler</c> established in
/// Sprint 1. Unlike Grammar, there is no module-local progress rollup to update here: the SM-2 schedule
/// on <c>VocabularyProgress</c> was already updated synchronously by <c>GradeVocabularyReviewCommandHandler</c>
/// before this event was raised, so this handler's only job is the cross-module report.
/// </summary>
public sealed class VocabularyCompletedHandler(IProgressTrackingService progressTracking, IClock clock)
    : INotificationHandler<DomainEventNotification<VocabularyCompletedDomainEvent>>
{
    public Task Handle(DomainEventNotification<VocabularyCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        return progressTracking.RecordSkillActivityAsync(
            domainEvent.TenantId, domainEvent.UserId, LearningSkill.Vocabulary, clock.UtcNow, cancellationToken);
    }
}
