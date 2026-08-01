using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Speaking.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>Reacts to <see cref="SpeakingEvaluatedDomainEvent"/> (Sprint 7) to complete the mock exam's Speaking section once it's linked (see <see cref="SpeakingSectionSubmittedHandler"/>).</summary>
public sealed class SpeakingSectionEvaluatedHandler(IMockExamRepository mockExams, ISender sender)
    : INotificationHandler<DomainEventNotification<SpeakingEvaluatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SpeakingEvaluatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.SpeakingAttemptId != domainEvent.SpeakingAttemptId)
        {
            return;
        }

        await sender.Send(
            new CompleteMockExamSectionCommand(domainEvent.TenantId, domainEvent.UserId, mockExam.Id.Value, MockExamSectionType.Speaking, domainEvent.EstimatedBand),
            cancellationToken).ConfigureAwait(false);
    }
}
