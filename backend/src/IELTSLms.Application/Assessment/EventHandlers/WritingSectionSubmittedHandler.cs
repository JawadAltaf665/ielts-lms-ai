using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Writing.Events;
using MediatR;

namespace IELTSLms.Application.Assessment.EventHandlers;

/// <summary>
/// Reacts to <see cref="WritingSubmittedDomainEvent"/> (Sprint 6, raised but previously unhandled) to
/// link the resulting <c>WritingSubmissionId</c> onto the caller's active mock exam, when its Writing
/// section is started-but-not-yet-linked. Writing has no decoupled Start phase of its own (unlike
/// Reading/Listening), so this "match by single active session" is how the exam discovers which
/// submission belongs to it, without adding a <c>MockExamId</c> column to <c>WritingSubmission</c>
/// (out of this sprint's scope — the Writing module is not touched).
/// </summary>
public sealed class WritingSectionSubmittedHandler(IMockExamRepository mockExams, IUnitOfWork unitOfWork, IClock clock)
    : INotificationHandler<DomainEventNotification<WritingSubmittedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<WritingSubmittedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var mockExam = await mockExams.GetActiveInProgressByUserAsync(domainEvent.TenantId, domainEvent.UserId, cancellationToken).ConfigureAwait(false);
        if (mockExam is null)
        {
            return;
        }

        var linkResult = mockExam.LinkSectionAttempt(MockExamSectionType.Writing, domainEvent.WritingSubmissionId, clock.UtcNow);
        if (linkResult.IsFailure)
        {
            // Writing section not started for this exam (or already linked) - an ordinary, unrelated
            // writing submission made while a mock exam happens to be active. Not this handler's concern.
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
