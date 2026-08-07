using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class WorkflowRunRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WorkflowRun, WorkflowRunId>(dbContext), IWorkflowRunRepository
{
    public async Task<WorkflowRun?> GetLatestByTriggerRefAsync(
        Guid tenantId, string workflowKey, string triggerRef, CancellationToken cancellationToken = default) =>
        await Set
            .Where(w => w.TenantId == tenantId
                        && w.WorkflowKey == workflowKey
                        && w.TriggerRef == triggerRef
                        && !w.IsDeleted)
            .OrderByDescending(w => w.StartedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<WorkflowRun?> GetByIdForTenantAsync(
        Guid tenantId, WorkflowRunId id, CancellationToken cancellationToken = default) =>
        await Set
            .Where(w => w.Id == id && w.TenantId == tenantId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
}
