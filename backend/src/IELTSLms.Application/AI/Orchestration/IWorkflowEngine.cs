using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Executes a <see cref="WorkflowDefinition"/> step-by-step, persisting progress as a
/// <see cref="WorkflowRun"/> (Sprint 10, Workflow Engine).
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Executes all steps in <paramref name="definition"/> against <paramref name="context"/>,
    /// returns the <see cref="WorkflowRunId"/> of the persisted audit record on success.
    /// On failure the run is persisted with <see cref="WorkflowRunStatus.Failed"/> or
    /// <see cref="WorkflowRunStatus.Compensated"/> and the error returned via <see cref="Result"/>.
    /// </summary>
    Task<Result<WorkflowRunId>> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowContext context,
        CancellationToken cancellationToken = default);
}
