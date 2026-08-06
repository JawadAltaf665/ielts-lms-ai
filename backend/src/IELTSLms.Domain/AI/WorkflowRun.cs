using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// Audit record for one executed workflow instance (Sprint 10, Agentic AI Architecture §6).
/// Tenant-scoped and append-mostly — created in <see cref="WorkflowRunStatus.Running"/> when the
/// Workflow Engine starts, then transitioned to <see cref="WorkflowRunStatus.Completed"/>,
/// <see cref="WorkflowRunStatus.Failed"/>, or <see cref="WorkflowRunStatus.Compensated"/> when it
/// finishes. Step-level results are stored in <see cref="StepTraceJson"/> as a structured JSON blob
/// so completed step outputs remain retrievable even on partial failure (business rule: a partial
/// mock-exam review is visible and re-runnable per section, not silently discarded).
///
/// Mirrors <see cref="AiOperation"/>'s tenant+time index discipline — queries on this table go through
/// <c>(TenantId, WorkflowKey)</c> or <c>(TenantId, TriggerRef)</c>, never unscoped.
/// </summary>
public sealed class WorkflowRun : TenantAuditableEntity<WorkflowRunId>
{
    /// <summary>Maximum length of the workflow key slug (e.g. <c>"monthly-mock-exam"</c>).</summary>
    public const int WorkflowKeyMaxLength = 64;

    /// <summary>Maximum length of the trigger-reference string (e.g. a MockExamId guid).</summary>
    public const int TriggerRefMaxLength = 128;

    private WorkflowRun(
        WorkflowRunId id,
        Guid tenantId,
        Guid userId,
        string workflowKey,
        string triggerRef,
        DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        WorkflowKey = workflowKey;
        TriggerRef = triggerRef;
        Status = WorkflowRunStatus.Running;
        StartedAt = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private WorkflowRun()
    {
    }

    /// <summary>The user that triggered this workflow run.</summary>
    public Guid UserId { get; private init; }

    /// <summary>Stable slug identifying the workflow type (e.g. <c>"monthly-mock-exam"</c>).</summary>
    public string WorkflowKey { get; private init; } = string.Empty;

    /// <summary>Opaque reference to the entity that triggered the run (e.g. the MockExam's Id).
    /// Stored as a string so it can reference any aggregate without a typed FK.</summary>
    public string TriggerRef { get; private init; } = string.Empty;

    /// <summary>Current lifecycle status of the run.</summary>
    public WorkflowRunStatus Status { get; private set; }

    /// <summary>UTC timestamp when the run started (set in the factory, never null).</summary>
    public DateTimeOffset StartedAt { get; private init; }

    /// <summary>UTC timestamp when the run reached a terminal state. Null while running.</summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Per-step structured JSON trace: a JSON array where each element records the step key, outcome,
    /// output JSON (if succeeded), and error (if failed). Appended atomically on terminal transition
    /// so partial results survive a failure. Null until the run reaches a terminal state.
    /// </summary>
    public string? StepTraceJson { get; private set; }

    /// <summary>Creates a <see cref="WorkflowRun"/> in <see cref="WorkflowRunStatus.Running"/> state.
    /// The entity is persisted immediately so the client has a resource to poll.</summary>
    public static Result<WorkflowRun> Begin(
        WorkflowRunId id, Guid tenantId, Guid userId, string workflowKey, string triggerRef, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(workflowKey) || workflowKey.Length > WorkflowKeyMaxLength)
        {
            return Result.Failure<WorkflowRun>(WorkflowRunDomainErrors.InvalidWorkflowKey);
        }

        if (string.IsNullOrWhiteSpace(triggerRef) || triggerRef.Length > TriggerRefMaxLength)
        {
            return Result.Failure<WorkflowRun>(WorkflowRunDomainErrors.InvalidTriggerRef);
        }

        return Result.Success(new WorkflowRun(id, tenantId, userId, workflowKey, triggerRef, nowUtc));
    }

    /// <summary>Transitions to <see cref="WorkflowRunStatus.Completed"/>. Idempotent if already terminal.</summary>
    public void Complete(string stepTraceJson, DateTimeOffset nowUtc)
    {
        if (Status != WorkflowRunStatus.Running)
        {
            return;
        }

        Status = WorkflowRunStatus.Completed;
        StepTraceJson = stepTraceJson;
        CompletedAt = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Transitions to <see cref="WorkflowRunStatus.Failed"/>. Persists the partial step trace
    /// so completed steps' results remain retrievable (business rule).</summary>
    public void Fail(string stepTraceJson, DateTimeOffset nowUtc)
    {
        if (Status != WorkflowRunStatus.Running)
        {
            return;
        }

        Status = WorkflowRunStatus.Failed;
        StepTraceJson = stepTraceJson;
        CompletedAt = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Transitions to <see cref="WorkflowRunStatus.Compensated"/>. Used when saga-style
    /// compensation callbacks ran successfully after a step error.</summary>
    public void Compensate(string stepTraceJson, DateTimeOffset nowUtc)
    {
        if (Status != WorkflowRunStatus.Running)
        {
            return;
        }

        Status = WorkflowRunStatus.Compensated;
        StepTraceJson = stepTraceJson;
        CompletedAt = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}

internal static class WorkflowRunDomainErrors
{
    public static readonly Error InvalidWorkflowKey = Error.Validation(
        "VALIDATION_WORKFLOW_RUN_INVALID_KEY",
        $"WorkflowKey must be a non-empty string of at most {WorkflowRun.WorkflowKeyMaxLength} characters.");

    public static readonly Error InvalidTriggerRef = Error.Validation(
        "VALIDATION_WORKFLOW_RUN_INVALID_TRIGGER_REF",
        $"TriggerRef must be a non-empty string of at most {WorkflowRun.TriggerRefMaxLength} characters.");
}
