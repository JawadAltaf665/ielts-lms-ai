using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Orchestration.Workflows;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Queries.GetOrchestratedReview;

public sealed class GetOrchestratedReviewQueryHandler(
    IMockExamRepository mockExams,
    IWorkflowRunRepository workflowRuns)
    : IQueryHandler<GetOrchestratedReviewQuery, OrchestratedReviewResponse>
{
    public async Task<Result<OrchestratedReviewResponse>> Handle(
        GetOrchestratedReviewQuery request, CancellationToken cancellationToken)
    {
        // 1. Validate the mock exam exists and belongs to the user.
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<OrchestratedReviewResponse>(MockExamErrors.NotFound);
        }

        // 2. Fetch the latest orchestrated review run for this exam.
        var run = await workflowRuns.GetLatestByTriggerRefAsync(
            request.TenantId,
            MonthlyMockExamWorkflow.Key,
            request.MockExamId.ToString(),
            cancellationToken).ConfigureAwait(false);

        if (run is null)
        {
            return Result.Failure<OrchestratedReviewResponse>(MockExamErrors.OrchestratedReviewNotFound);
        }

        return Result.Success(new OrchestratedReviewResponse(
            run.Id.Value,
            run.Status.ToString(),
            run.StartedAt,
            run.CompletedAt,
            run.StepTraceJson));
    }
}
