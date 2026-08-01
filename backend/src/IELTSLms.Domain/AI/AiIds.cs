namespace IELTSLms.Domain.AI;

// Strongly-typed identifiers for the AI bounded context (BC-14), grouped in one file per the
// Billing/Grammar convention (BillingIds.cs, GrammarIds.cs) — one-file-per-id doesn't scale without
// pure ceremony.

/// <summary>Strongly-typed identifier for an <see cref="AiProvider"/>.</summary>
public readonly record struct AiProviderId(Guid Value)
{
    public static AiProviderId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="AiPrompt"/>.</summary>
public readonly record struct AiPromptId(Guid Value)
{
    public static AiPromptId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="AiResponse"/>.</summary>
public readonly record struct AiResponseId(Guid Value)
{
    public static AiResponseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <see cref="AiOperation"/>.</summary>
public readonly record struct AiOperationId(Guid Value)
{
    public static AiOperationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="BandPrediction"/>.</summary>
public readonly record struct BandPredictionId(Guid Value)
{
    public static BandPredictionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
