using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Reading.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>
/// Reacts to <see cref="ReadingAttemptScoredDomainEvent"/> (Sprint 4, already raised — no changes to
/// the Reading module) to complete the mock exam's Reading section, when the scored attempt is the one
/// the caller's active mock exam started. A second, independent subscriber alongside Reading's own
/// <c>ReadingAttemptScoredHandler</c> — MediatR fans a notification out to every handler.
/// </summary>
public sealed class ReadingSectionScoredHandler(IMockExamRepository mockExams, ISender sender)
    : INotificationHandler<DomainEventNotification<ReadingAttemptScoredDomainEvent>>
{
    public async Task Handle(DomainEventNotification<ReadingAttemptScoredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.ReadingAttemptId != domainEvent.ReadingAttemptId)
        {
            return;
        }

        await sender.Send(
            new CompleteMockExamSectionCommand(domainEvent.TenantId, domainEvent.UserId, mockExam.Id.Value, MockExamSectionType.Reading, domainEvent.Band),
            cancellationToken).ConfigureAwait(false);
    }
}
