using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>
/// Aggregate access for <see cref="WorkflowRun"/> (Sprint 10, Workflow Engine).
/// Tenant-scoped — every query enforces the tenant boundary (cross-tenant access returns null/empty,
/// never 403, per the project's INV-04 rule).
/// </summary>
public interface IWorkflowRunRepository : IRepository<WorkflowRun, WorkflowRunId>
{
    /// <summary>
    /// Returns the most recent <see cref="WorkflowRun"/> for the given trigger reference (e.g. a
    /// MockExamId) within a tenant. Used by <c>GetOrchestratedReviewQueryHandler</c> to locate the
    /// latest completed run for a mock exam.
    /// </summary>
    Task<WorkflowRun?> GetLatestByTriggerRefAsync(
        Guid tenantId, string workflowKey, string triggerRef, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single <see cref="WorkflowRun"/> by id, enforcing tenant scope.
    /// Cross-tenant access returns null (404, not 403).
    /// </summary>
    Task<WorkflowRun?> GetByIdForTenantAsync(
        Guid tenantId, WorkflowRunId id, CancellationToken cancellationToken = default);
}
