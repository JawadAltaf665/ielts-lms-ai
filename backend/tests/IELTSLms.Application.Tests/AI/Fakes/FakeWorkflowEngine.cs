using IELTSLms.Application.AI.Orchestration;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeWorkflowEngine : IWorkflowEngine
{
    private readonly List<(WorkflowDefinition Definition, WorkflowContext Context)> _executions = [];
    public IReadOnlyList<(WorkflowDefinition Definition, WorkflowContext Context)> Executions => _executions;

    public Result<WorkflowRunId> ForcedResult { get; set; } = Result.Success(WorkflowRunId.From(Guid.NewGuid()));

    public Task<Result<WorkflowRunId>> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        _executions.Add((definition, context));
        return Task.FromResult(ForcedResult);
    }
}
