using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Grammar.Events;
using IELTSLms.Domain.Learning;
using MediatR;

namespace IELTSLms.Application.Grammar.EventHandlers;

/// <summary>
/// Reacts to a scored attempt (DDD §11.2 POL-04-style generalization): updates Grammar's own
/// per-topic <see cref="GrammarProgress"/>, then reports the activity to the shared, context-agnostic
/// <see cref="IProgressTrackingService"/>. This is the first such handler in the codebase — every
/// later skill module wires its own <c>&lt;Skill&gt;Completed</c> event the same way.
/// </summary>
public sealed class GrammarCompletedHandler(
    IGrammarProgressRepository grammarProgress, IUnitOfWork unitOfWork, IProgressTrackingService progressTracking,
    IClock clock, IGuidProvider guids)
    : INotificationHandler<DomainEventNotification<GrammarCompletedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<GrammarCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var now = clock.UtcNow;
        var topicId = GrammarTopicId.From(domainEvent.GrammarTopicId);

        var progress = await grammarProgress.GetByUserAndTopicAsync(domainEvent.TenantId, domainEvent.UserId, topicId, cancellationToken)
            .ConfigureAwait(false);
        if (progress is null)
        {
            progress = GrammarProgress.Create(
                GrammarProgressId.From(guids.NewGuid()), domainEvent.TenantId, domainEvent.UserId, topicId, now);
            grammarProgress.Add(progress);
        }

        progress.RecordAttempt(domainEvent.RawScore, domainEvent.MaxScore, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await progressTracking.RecordSkillActivityAsync(domainEvent.TenantId, domainEvent.UserId, LearningSkill.Grammar, now, cancellationToken)
            .ConfigureAwait(false);
    }
}
