using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeWorkflowRunRepository : IWorkflowRunRepository
{
    private readonly List<WorkflowRun> _runs = [];

    public IReadOnlyList<WorkflowRun> Runs => _runs;

    public void Seed(WorkflowRun run) => _runs.Add(run);

    public Task<WorkflowRun?> GetByIdAsync(WorkflowRunId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_runs.FirstOrDefault(r => r.Id == id));

    public Task<WorkflowRun?> GetByIdForTenantAsync(
        Guid tenantId, WorkflowRunId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_runs.FirstOrDefault(r => r.TenantId == tenantId && r.Id == id));

    public Task<WorkflowRun?> GetLatestByTriggerRefAsync(
        Guid tenantId, string workflowKey, string triggerRef, CancellationToken cancellationToken = default) =>
        Task.FromResult(_runs
            .Where(r => r.TenantId == tenantId && r.WorkflowKey == workflowKey && r.TriggerRef == triggerRef)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault());

    public void Add(WorkflowRun aggregate) => _runs.Add(aggregate);

    public void Remove(WorkflowRun aggregate) => _runs.Remove(aggregate);
}
