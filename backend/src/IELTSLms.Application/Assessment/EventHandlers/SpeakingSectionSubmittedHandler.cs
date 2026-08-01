using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Speaking.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>Reacts to <see cref="SpeakingSubmittedDomainEvent"/> (Sprint 7, raised but previously unhandled) — same "match by active session" linking as <see cref="WritingSectionSubmittedHandler"/>.</summary>
public sealed class SpeakingSectionSubmittedHandler(IMockExamRepository mockExams, IUnitOfWork unitOfWork, IClock clock)
    : INotificationHandler<DomainEventNotification<SpeakingSubmittedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SpeakingSubmittedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null)
        {
            return;
        }

        var linkResult = mockExam.LinkSectionAttempt(MockExamSectionType.Speaking, domainEvent.SpeakingAttemptId, clock.UtcNow);
        if (linkResult.IsFailure)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
