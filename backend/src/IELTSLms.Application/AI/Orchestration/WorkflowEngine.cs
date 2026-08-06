using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Step-graph executor for the Sprint 10 Workflow Engine (Agentic AI Architecture §6).
///
/// Execution rules:
/// <list type="bullet">
///   <item>Adjacent <see cref="StepKind.Parallel"/> steps are grouped into a single
///         <c>Task.WhenAll</c> fanout. The group ends at the first non-Parallel step or end-of-list.</item>
///   <item><see cref="StepKind.Sequential"/> and <see cref="StepKind.Conditional"/> steps run one at a
///         time. Conditional steps whose predicate returns false are skipped (not failed).</item>
///   <item>Per-step retries: up to <see cref="StepDefinition.MaxRetries"/> additional attempts with a
///         fixed 500ms delay. AI Gateway's own retry is not duplicated here — this layer retries
///         transient application errors, not provider-level transport errors.</item>
///   <item>First step failure: remaining steps are skipped; compensation callbacks run in reverse order
///         for all previously-succeeded steps. If all compensations succeed, the run transitions to
///         <see cref="WorkflowRunStatus.Compensated"/>; otherwise to <see cref="WorkflowRunStatus.Failed"/>.</item>
///   <item>Budget guard: throws <see cref="InvalidOperationException"/> if the step count exceeds
///         <see cref="MaxSteps"/> — prevents infinite loops in pathological definitions.</item>
/// </list>
/// </summary>
public sealed partial class WorkflowEngine(
    IWorkflowRunRepository workflowRuns,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    ILogger<WorkflowEngine> logger) : IWorkflowEngine
{
    /// <summary>Hard ceiling on step count per workflow execution. Definitions exceeding this are
    /// rejected at the start of execution, not at step N — fail fast, not deep.</summary>
    public const int MaxSteps = 32;

    /// <summary>Base retry delay. Kept short since the engine layer's retries target transient
    /// application failures, not slow AI provider calls.</summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    public async Task<Result<WorkflowRunId>> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        if (definition.Steps.Count > MaxSteps)
        {
            return Result.Failure<WorkflowRunId>(WorkflowErrors.BudgetExceeded);
        }

        var now = clock.UtcNow;
        var beginResult = WorkflowRun.Begin(
            WorkflowRunId.From(guids.NewGuid()),
            context.TenantId,
            context.UserId,
            definition.Key,
            context.Get<string>(WorkflowContextKeys.TriggerRef) ?? definition.Key,
            now);

        if (beginResult.IsFailure)
        {
            return Result.Failure<WorkflowRunId>(beginResult.Error);
        }

        var run = beginResult.Value;
        workflowRuns.Add(run);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        WorkflowStarted(logger, definition.Key, run.Id.Value);

        try
        {
            await ExecuteStepsAsync(definition.Steps, context, run, cancellationToken).ConfigureAwait(false);
            return Result.Success(run.Id);
        }
        catch (OperationCanceledException)
        {
            var traceJson = SerializeTrace(context.StepTraces);
            run.Fail(traceJson, clock.UtcNow);
            await unitOfWork.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            return Result.Failure<WorkflowRunId>(WorkflowErrors.Cancelled);
        }
    }

    private async Task ExecuteStepsAsync(
        IReadOnlyList<StepDefinition> steps,
        WorkflowContext context,
        WorkflowRun run,
        CancellationToken cancellationToken)
    {
        var succeeded = new List<StepDefinition>();

        var i = 0;
        while (i < steps.Count)
        {
            var step = steps[i];

            // Collect adjacent parallel steps into a group.
            if (step.Kind == StepKind.Parallel)
            {
                var group = new List<StepDefinition> { step };
                var j = i + 1;
                while (j < steps.Count && steps[j].Kind == StepKind.Parallel)
                {
                    group.Add(steps[j]);
                    j++;
                }

                var groupResult = await ExecuteParallelGroupAsync(group, context, cancellationToken).ConfigureAwait(false);
                if (!groupResult)
                {
                    await HandleFailureAsync(succeeded, context, run, cancellationToken).ConfigureAwait(false);
                    return;
                }

                succeeded.AddRange(group);
                i = j;
                continue;
            }

            // Conditional: skip if predicate is false.
            if (step.Kind == StepKind.Conditional && step.Condition is not null && !step.Condition(context))
            {
                StepSkipped(logger, step.StepKey);
                i++;
                continue;
            }

            // Sequential (or conditional that passed predicate): run one step.
            var result = await ExecuteStepWithRetryAsync(step, context, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                await HandleFailureAsync(succeeded, context, run, cancellationToken).ConfigureAwait(false);
                return;
            }

            succeeded.Add(step);
            i++;
        }

        // All steps succeeded.
        var traceJson = SerializeTrace(context.StepTraces);
        run.Complete(traceJson, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        WorkflowCompleted(logger, run.WorkflowKey, run.Id.Value);
    }

    private async Task<bool> ExecuteParallelGroupAsync(
        IReadOnlyList<StepDefinition> group,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var tasks = group.Select(step => ExecuteStepWithRetryAsync(step, context, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.All(r => r);
    }

    /// <returns><see langword="true"/> if the step succeeded (possibly after retries).</returns>
    private async Task<bool> ExecuteStepWithRetryAsync(
        StepDefinition step,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var attempts = step.MaxRetries + 1;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            var startedAt = clock.UtcNow;
            try
            {
                var result = await step.Execute(context, cancellationToken).ConfigureAwait(false);
                var completedAt = clock.UtcNow;
                var trace = new StepTrace(step.StepKey, result.Succeeded, result.OutputJson, result.ErrorCode, result.ErrorMessage, startedAt, completedAt);
                context.AppendTrace(trace);

                if (result.Succeeded)
                {
                    StepSucceeded(logger, step.StepKey, attempt);
                    return true;
                }

                StepFailed(logger, step.StepKey, attempt, result.ErrorCode ?? "unknown");

                if (attempt < attempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var completedAt = clock.UtcNow;
                var trace = new StepTrace(step.StepKey, false, null, "EXCEPTION", ex.Message, startedAt, completedAt);
                context.AppendTrace(trace);
                StepException(logger, step.StepKey, attempt, ex);

                if (attempt < attempts)
                {
                    await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return false;
    }

    private async Task HandleFailureAsync(
        IReadOnlyList<StepDefinition> succeededSteps,
        WorkflowContext context,
        WorkflowRun run,
        CancellationToken cancellationToken)
    {
        var compensationSucceeded = true;

        // Compensate in reverse order of succeeded steps.
        foreach (var step in succeededSteps.Reverse())
        {
            if (step.Compensate is null)
            {
                continue;
            }

            try
            {
                await step.Compensate(context, cancellationToken).ConfigureAwait(false);
                CompensationSucceeded(logger, step.StepKey);
            }
            catch (Exception ex)
            {
                CompensationFailed(logger, step.StepKey, ex);
                compensationSucceeded = false;
            }
        }

        var traceJson = SerializeTrace(context.StepTraces);

        if (compensationSucceeded && succeededSteps.Any(s => s.Compensate is not null))
        {
            run.Compensate(traceJson, clock.UtcNow);
            WorkflowCompensated(logger, run.WorkflowKey, run.Id.Value);
        }
        else
        {
            run.Fail(traceJson, clock.UtcNow);
            WorkflowFailed(logger, run.WorkflowKey, run.Id.Value);
        }

        // Use CancellationToken.None — we must persist the terminal state even if the token was cancelled.
        await unitOfWork.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private static string SerializeTrace(IEnumerable<StepTrace> traces) =>
        JsonSerializer.Serialize(traces);

    // --- Structured log messages ---

    [LoggerMessage(Level = LogLevel.Information, Message = "Workflow {WorkflowKey} started (run {RunId}).")]
    private static partial void WorkflowStarted(ILogger logger, string workflowKey, Guid runId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Workflow {WorkflowKey} completed successfully (run {RunId}).")]
    private static partial void WorkflowCompleted(ILogger logger, string workflowKey, Guid runId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Workflow {WorkflowKey} failed (run {RunId}).")]
    private static partial void WorkflowFailed(ILogger logger, string workflowKey, Guid runId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Workflow {WorkflowKey} failed with compensation (run {RunId}).")]
    private static partial void WorkflowCompensated(ILogger logger, string workflowKey, Guid runId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepKey} succeeded on attempt {Attempt}.")]
    private static partial void StepSucceeded(ILogger logger, string stepKey, int attempt);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Step {StepKey} failed on attempt {Attempt} ({ErrorCode}).")]
    private static partial void StepFailed(ILogger logger, string stepKey, int attempt, string errorCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "Step {StepKey} threw an exception on attempt {Attempt}.")]
    private static partial void StepException(ILogger logger, string stepKey, int attempt, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Step {StepKey} skipped (condition returned false).")]
    private static partial void StepSkipped(ILogger logger, string stepKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Compensation for step {StepKey} succeeded.")]
    private static partial void CompensationSucceeded(ILogger logger, string stepKey);

    [LoggerMessage(Level = LogLevel.Error, Message = "Compensation for step {StepKey} failed.")]
    private static partial void CompensationFailed(ILogger logger, string stepKey, Exception ex);
}

/// <summary>Well-known context keys shared between the engine and workflow definitions.</summary>
public static class WorkflowContextKeys
{
    /// <summary>The opaque string reference to the entity that triggered the workflow
    /// (e.g. a MockExam Id). Set by the caller before invoking <see cref="IWorkflowEngine"/>.</summary>
    public const string TriggerRef = "trigger_ref";
}

internal static class WorkflowErrors
{
    public static readonly Error BudgetExceeded = Error.Validation(
        "WORKFLOW_BUDGET_EXCEEDED",
        $"Workflow definition exceeds the maximum of {WorkflowEngine.MaxSteps} steps.");

    public static readonly Error Cancelled = Error.Validation(
        "WORKFLOW_CANCELLED",
        "The workflow execution was cancelled.");
}
