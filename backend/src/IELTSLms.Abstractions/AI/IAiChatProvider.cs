namespace IELTSLms.Abstractions.AI;

/// <summary>
/// Provider-agnostic chat/completion port (AI Architecture §2–3). Implemented once per vendor in
/// Infrastructure/AI (Claude to start); the domain and Application layer never see vendor-specific
/// request/response shapes — that translation is the adapter's anti-corruption-layer job.
/// </summary>
public interface IAiChatProvider
{
    Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// A single structured-output completion request. <paramref name="ResponseSchemaJson"/> is a JSON
/// Schema the provider is asked to conform its output to (AI Architecture §8); how a given vendor
/// enforces that (tool-use, JSON mode, ...) is the adapter's concern.
/// </summary>
public sealed record AiChatRequest(
    string SystemPrompt,
    string UserContent,
    string ResponseSchemaName,
    string ResponseSchemaJson,
    int MaxOutputTokens);

/// <summary>Raw provider result before schema validation. <see cref="StructuredJson"/> is the provider's
/// structured-output payload as text, not yet parsed/validated by the caller. Cost is reported by the
/// adapter itself (each vendor knows its own pricing) so the gateway stays provider-agnostic.</summary>
public sealed record AiChatResult(
    string StructuredJson,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    long CostMinorUnits,
    string CostCurrency,
    string? ProviderResponseId);
