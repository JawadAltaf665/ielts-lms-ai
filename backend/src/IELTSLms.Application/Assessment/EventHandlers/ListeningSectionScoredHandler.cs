using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Listening.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>Reacts to <see cref="ListeningAttemptScoredDomainEvent"/> (Sprint 5) — same pattern as <see cref="ReadingSectionScoredHandler"/>.</summary>
public sealed class ListeningSectionScoredHandler(IMockExamRepository mockExams, ISender sender)
    : INotificationHandler<DomainEventNotification<ListeningAttemptScoredDomainEvent>>
{
    public async Task Handle(DomainEventNotification<ListeningAttemptScoredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.ListeningAttemptId != domainEvent.ListeningAttemptId)
        {
            return;
        }

        await sender.Send(
            new CompleteMockExamSectionCommand(domainEvent.TenantId, domainEvent.UserId, mockExam.Id.Value, MockExamSectionType.Listening, domainEvent.Band),
            cancellationToken).ConfigureAwait(false);
    }
}
