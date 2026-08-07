using IELTSLms.Abstractions.Events;
using IELTSLms.Application.AI.Orchestration.Commands.RunMockExamOrchestratedReview;
using IELTSLms.Domain.Assessment.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.EventHandlers;

/// <summary>
/// Reacts to a completed mock exam by triggering the Monthly Mock Exam Workflow (Sprint 10).
/// The orchestrated review aggregates results and runs AI analysis (Band Predictor, Learning Recommendation).
/// </summary>
public sealed partial class MockExamCompletedHandler(
    ISender sender,
    ILogger<MockExamCompletedHandler> logger)
    : INotificationHandler<DomainEventNotification<MockExamCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<MockExamCompletedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        ReviewTriggered(logger, domainEvent.MockExamId);

        var command = new RunMockExamOrchestratedReviewCommand(
            domainEvent.TenantId,
            domainEvent.UserId,
            domainEvent.MockExamId,
            domainEvent.OverallBand);

        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            ReviewTriggerFailed(logger, domainEvent.MockExamId, result.Error);
        }
        else
        {
            ReviewStarted(logger, result.Value.Value, domainEvent.MockExamId);
        }
    }

    [LoggerMessage(EventId = 4100, Level = LogLevel.Information, Message = "MockExamCompletedDomainEvent received for exam {MockExamId}. Triggering orchestrated review.")]
    private static partial void ReviewTriggered(ILogger logger, Guid mockExamId);

    [LoggerMessage(EventId = 4101, Level = LogLevel.Error, Message = "Failed to start orchestrated review for exam {MockExamId}: {Error}")]
    private static partial void ReviewTriggerFailed(ILogger logger, Guid mockExamId, IELTSLms.SharedKernel.Results.Error error);

    [LoggerMessage(EventId = 4102, Level = LogLevel.Information, Message = "Orchestrated review {WorkflowRunId} started for exam {MockExamId}.")]
    private static partial void ReviewStarted(ILogger logger, Guid workflowRunId, Guid mockExamId);
}
