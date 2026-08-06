namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Mutable context bag threaded through every step of a workflow execution (Sprint 10, Workflow Engine).
/// Holds the initial inputs and accumulates step outputs as each step completes. Steps read from and
/// write to this context; the engine persists the accumulated trace to
/// <see cref="Domain.AI.WorkflowRun.StepTraceJson"/> on completion.
/// </summary>
public sealed class WorkflowContext
{
    private readonly Dictionary<string, object?> _state = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<StepTrace> _trace = [];

    /// <summary>Creates a new context with the given tenant/user scope and named inputs.</summary>
    public WorkflowContext(Guid tenantId, Guid userId, IReadOnlyDictionary<string, object?> inputs)
    {
        TenantId = tenantId;
        UserId = userId;

        foreach (var (key, value) in inputs)
        {
            _state[key] = value;
        }
    }

    /// <summary>The tenant this workflow run is executing within.</summary>
    public Guid TenantId { get; }

    /// <summary>The user who triggered this workflow run.</summary>
    public Guid UserId { get; }

    /// <summary>Step traces appended as each step completes. Serialized into
    /// <see cref="Domain.AI.WorkflowRun.StepTraceJson"/> on terminal transition.</summary>
    public IReadOnlyList<StepTrace> StepTraces => _trace;

    /// <summary>Retrieves a value set by a prior step (or an initial input). Returns
    /// <see langword="default"/> if the key has not been set.</summary>
    public T? Get<T>(string key) =>
        _state.TryGetValue(key, out var value) ? (T?)value : default;

    /// <summary>Sets (or overwrites) a named value in the context.
    /// Called by the step delegate to publish its output to downstream steps.</summary>
    public void Set(string key, object? value) => _state[key] = value;

    /// <summary>Returns true if the named key has a non-null value.</summary>
    public bool Has(string key) => _state.TryGetValue(key, out var v) && v is not null;

    /// <summary>Appends a step trace entry. Called by the engine after each step completes.</summary>
    internal void AppendTrace(StepTrace trace) => _trace.Add(trace);
}

/// <summary>Immutable record of one step's execution outcome, accumulated in the context and later
/// serialized to <see cref="Domain.AI.WorkflowRun.StepTraceJson"/>.</summary>
public sealed record StepTrace(
    string StepKey,
    bool Succeeded,
    string? OutputJson,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt);
