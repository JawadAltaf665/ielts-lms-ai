namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Declarative step-graph definition for a workflow (Sprint 10, Workflow Engine). A plain record with
/// no runtime machinery — the engine interprets it; workflows don't subclass the engine.
/// </summary>
/// <param name="Key">Stable slug identifying this workflow type (e.g. <c>"monthly-mock-exam"</c>).
/// Stored in <see cref="Domain.AI.WorkflowRun.WorkflowKey"/>.</param>
/// <param name="Steps">Ordered steps. Sequential steps run in order; parallel groups are defined by
/// <see cref="StepKind.Parallel"/> steps that share the same position in the step list (see remarks
/// on <see cref="WorkflowEngine"/> for the grouping rule).</param>
public sealed record WorkflowDefinition(string Key, IReadOnlyList<StepDefinition> Steps);

/// <summary>
/// Definition of one step within a <see cref="WorkflowDefinition"/>.
/// </summary>
/// <param name="StepKey">Stable slug identifying this step (e.g. <c>"writing-examiner"</c>).
/// Used as the key in <see cref="StepTrace"/>.</param>
/// <param name="Kind">Execution mode for this step.</param>
/// <param name="Execute">The step body. Must be idempotent (retry may call it more than once).</param>
/// <param name="MaxRetries">Number of additional attempts after the first failure.
/// 0 means try exactly once (no retry). The engine uses a simple linear delay between retries.</param>
/// <param name="Compensate">Optional saga-style cleanup callback invoked (in reverse step order) if the
/// workflow fails after this step has succeeded. Null means no cleanup needed for this step.</param>
/// <param name="Condition">For <see cref="StepKind.Conditional"/> steps: a predicate evaluated against
/// the context; if it returns false the step is skipped (not failed). Null means always execute.</param>
public sealed record StepDefinition(
    string StepKey,
    StepKind Kind,
    Func<WorkflowContext, CancellationToken, Task<StepResult>> Execute,
    int MaxRetries = 0,
    Func<WorkflowContext, CancellationToken, Task>? Compensate = null,
    Func<WorkflowContext, bool>? Condition = null);

/// <summary>Execution mode of a <see cref="StepDefinition"/>.</summary>
public enum StepKind
{
    /// <summary>The step runs after the previous step completes.</summary>
    Sequential = 1,

    /// <summary>The step runs concurrently with adjacent <see cref="Parallel"/> steps
    /// (grouped by the engine into a single <c>Task.WhenAll</c> fanout).</summary>
    Parallel = 2,

    /// <summary>The step runs sequentially but is skipped if its
    /// <see cref="StepDefinition.Condition"/> returns false.</summary>
    Conditional = 3,
}

/// <summary>The outcome of a single step execution.</summary>
public sealed record StepResult(
    bool Succeeded,
    string? OutputJson = null,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    /// <summary>Convenience factory for a successful step with a JSON output.</summary>
    public static StepResult Success(string? outputJson = null) => new(true, OutputJson: outputJson);

    /// <summary>Convenience factory for a failed step with an error code.</summary>
    public static StepResult Failure(string errorCode, string? errorMessage = null) =>
        new(false, ErrorCode: errorCode, ErrorMessage: errorMessage);
}
