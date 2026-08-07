using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Orchestration.Queries.GetWorkflowRun;

/// <summary>
/// Retrieves the status and trace of a specific workflow run by ID.
/// </summary>
public sealed record GetWorkflowRunQuery(Guid TenantId, Guid UserId, Guid WorkflowRunId) : IQuery<WorkflowRunResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.workflow.read"];
}

public sealed record WorkflowRunResponse(
    Guid Id,
    string WorkflowKey,
    string TriggerRef,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? StepTraceJson);
