using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Writing.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>Reacts to <see cref="WritingEvaluatedDomainEvent"/> (Sprint 6/AI Gateway) to complete the mock exam's Writing section once it's linked (see <see cref="WritingSectionSubmittedHandler"/>).</summary>
public sealed class WritingSectionEvaluatedHandler(IMockExamRepository mockExams, ISender sender)
    : INotificationHandler<DomainEventNotification<WritingEvaluatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<WritingEvaluatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.WritingSubmissionId != domainEvent.WritingSubmissionId)
        {
            return;
        }

        await sender.Send(
            new CompleteMockExamSectionCommand(domainEvent.TenantId, domainEvent.UserId, mockExam.Id.Value, MockExamSectionType.Writing, domainEvent.EstimatedBand),
            cancellationToken).ConfigureAwait(false);
    }
}
