namespace IELTSLms.Domain.AI;

/// <summary>Vendor an <see cref="AiProvider"/> row talks to. Adding a provider is a new value plus an
/// Infrastructure/AI adapter, never a domain change (AI Architecture §3, ACL pattern).</summary>
public enum AiProviderKind
{
    Claude = 1,
}

/// <summary>Outcome of a recorded <see cref="AiResponse"/> (AI Architecture §8).</summary>
public enum AiResponseStatus
{
    Succeeded = 1,
    Failed = 2,

    /// <summary>Provider replied but the output failed schema validation after bounded repair/retry.</summary>
    Invalid = 3,
}

/// <summary>Lifecycle of an async AI capability invocation exposed via the operation-resource
/// endpoint (API Design Standards §14.2).</summary>
public enum AiOperationStatus
{
    Queued = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
}

/// <summary>Lifecycle of a <see cref="BandPrediction"/> (Agentic AI Architecture §13.6): created
/// pending alongside its <see cref="AiOperation"/>, then completed or failed by <c>BandPredictionJob</c>.</summary>
public enum BandPredictionStatus
{
    Pending = 1,
    Ready = 2,
    Failed = 3,
}
