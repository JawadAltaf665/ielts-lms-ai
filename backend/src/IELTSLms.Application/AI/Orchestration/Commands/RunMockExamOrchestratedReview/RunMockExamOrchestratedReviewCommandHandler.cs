using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestBandPrediction;
using IELTSLms.Application.Learning.StudyPlanner;
using IELTSLms.Application.AI.Orchestration.Workflows;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.AI.Orchestration.Commands.RunMockExamOrchestratedReview;

/// <summary>
/// Handles the internal <see cref="RunMockExamOrchestratedReviewCommand"/> that is published by
/// <see cref="EventHandlers.MockExamCompletedDomainEventHandler"/> on exam completion.
///
/// Assembles a <see cref="WorkflowContext"/> from the completed exam's data, then delegates to
/// <see cref="IWorkflowEngine"/> running the <see cref="MonthlyMockExamWorkflow"/>.
/// The resulting <see cref="WorkflowRun"/> is queryable via
/// <c>GET /api/v1/mock-exams/{id}/orchestrated-review</c>.
/// </summary>
public sealed class RunMockExamOrchestratedReviewCommandHandler(
    IMockExamResultRepository mockExamResults,
    IWorkflowEngine workflowEngine,
    DailyLearningWorkflowRunner learningRunner,
    ISender sender,
    IClock clock)
    : ICommandHandler<RunMockExamOrchestratedReviewCommand, WorkflowRunId>
{
    public async Task<Result<WorkflowRunId>> Handle(
        RunMockExamOrchestratedReviewCommand request, CancellationToken cancellationToken)
    {
        // Load the MockExamResult so workflow steps can read section bands from context.
        var mockExamResult = await mockExamResults
            .GetByMockExamIdAsync(MockExamId.From(request.MockExamId), cancellationToken)
            .ConfigureAwait(false);

        // Guard: orchestrated review only runs on finalized exams.
        if (mockExamResult is null || !mockExamResult.IsFinalized)
        {
            return Result.Failure<WorkflowRunId>(OrchestratedReviewErrors.ExamNotFinalized);
        }

        var inputs = new Dictionary<string, object?>
        {
            [WorkflowContextKeys.TriggerRef] = request.MockExamId.ToString(),
            [MonthlyMockExamWorkflowKeys.MockExamId] = request.MockExamId,
            [MonthlyMockExamWorkflowKeys.ReadingBand] = mockExamResult.ReadingBand,
            [MonthlyMockExamWorkflowKeys.ListeningBand] = mockExamResult.ListeningBand,
            [MonthlyMockExamWorkflowKeys.WritingBand] = mockExamResult.WritingBand,
            [MonthlyMockExamWorkflowKeys.SpeakingBand] = mockExamResult.SpeakingBand,
            [MonthlyMockExamWorkflowKeys.OverallBand] = request.OverallBand,
        };

        var context = new WorkflowContext(request.TenantId, request.UserId, inputs);

        var workflow = MonthlyMockExamWorkflow.Build(request.TenantId, request.UserId, sender, learningRunner, clock);

        return await workflowEngine.ExecuteAsync(workflow, context, cancellationToken).ConfigureAwait(false);
    }
}

internal static class OrchestratedReviewErrors
{
    public static readonly Error ExamNotFinalized = Error.NotFound(
        "NOT_FOUND_ORCHESTRATED_REVIEW_EXAM_NOT_FINALIZED",
        "The orchestrated review can only be triggered for a finalized mock exam.");
}
