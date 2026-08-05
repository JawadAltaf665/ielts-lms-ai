namespace IELTSLms.Domain.AI;

/// <summary>Vendor an <see cref="AiProvider"/> row talks to. Adding a provider is a new value plus an
/// Infrastructure/AI adapter, never a domain change (AI Architecture §3, ACL pattern).</summary>
public enum AiProviderKind
{
    Claude = 1,
    OpenAi = 2,
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

/// <summary>Which bounded context's question a <see cref="QuestionVariantDraft.SourceQuestionId"/>
/// polymorphically points at (Sprint 8) — the discriminator side of that reference, same pattern as
/// <see cref="AiResponse.RequestingEntityType"/>.</summary>
public enum QuestionVariantSourceType
{
    Reading = 1,
    Listening = 2,
}

/// <summary>Human-review lifecycle of a <see cref="QuestionVariantDraft"/> (Sprint 8, AI Architecture
/// §13 "generation assists, humans approve"): a draft is inert until a <c>content.manage</c> actor
/// explicitly approves it; a rejected draft is retained (status, not hard-deleted) for audit.</summary>
public enum QuestionVariantStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}
