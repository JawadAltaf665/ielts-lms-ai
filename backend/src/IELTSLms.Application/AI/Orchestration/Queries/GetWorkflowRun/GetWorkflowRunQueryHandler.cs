using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Orchestration.Queries.GetWorkflowRun;

public sealed class GetWorkflowRunQueryHandler(IWorkflowRunRepository workflowRuns)
    : IQueryHandler<GetWorkflowRunQuery, WorkflowRunResponse>
{
    public async Task<Result<WorkflowRunResponse>> Handle(
        GetWorkflowRunQuery request, CancellationToken cancellationToken)
    {
        var run = await workflowRuns.GetByIdAsync(WorkflowRunId.From(request.WorkflowRunId), cancellationToken).ConfigureAwait(false);

        if (run is null || run.TenantId != request.TenantId || run.UserId != request.UserId)
        {
            return Result.Failure<WorkflowRunResponse>(WorkflowRunErrors.NotFound);
        }

        return Result.Success(new WorkflowRunResponse(
            run.Id.Value,
            run.WorkflowKey,
            run.TriggerRef,
            run.Status.ToString(),
            run.StartedAt,
            run.CompletedAt,
            run.StepTraceJson));
    }
}

internal static class WorkflowRunErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "NOT_FOUND_WORKFLOW_RUN", "The specified workflow run was not found.");
}
