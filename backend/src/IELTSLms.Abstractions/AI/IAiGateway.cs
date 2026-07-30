using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Abstractions.AI;

/// <summary>
/// The single entry point Application-layer AI use-cases call (AI Architecture §2, §4) — routing,
/// rate-limit/quota concerns are handled by the caller via <c>IEntitlementService</c>/<c>IQuotaService</c>
/// before invocation; this port's implementation (Infrastructure/AI) owns caching, the safety/PII/
/// prompt-injection guard, the provider call, and structured-output validation. It never touches
/// persistence — the caller is responsible for recording the outcome as an <see cref="AiResponse"/>.
/// </summary>
public interface IAiGateway
{
    Task<Result<AiGatewayInvocationResult>> InvokeAsync(AiGatewayRequest request, CancellationToken cancellationToken = default);
}

/// <summary>A capability invocation request. <paramref name="SystemPrompt"/> is the already-resolved
/// <see cref="AiPrompt.SystemTemplate"/> text; the gateway never resolves prompts itself (AI
/// Architecture §5 — prompt resolution/versioning is Application's job, the gateway only executes).</summary>
public sealed record AiGatewayRequest(
    Guid TenantId,
    string Feature,
    int PromptVersion,
    AiProviderKind ProviderKind,
    string SystemPrompt,
    string UserInput,
    string ResponseSchemaName,
    string ResponseSchemaJson,
    int MaxOutputTokens);

/// <summary>Everything the caller needs to persist an <see cref="AiResponse"/>. <see cref="FromCache"/>
/// distinguishes a cache hit (zero provider tokens/cost) from a live call.</summary>
public sealed record AiGatewayInvocationResult(
    string OutputJson,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    int LatencyMs,
    long CostMinorUnits,
    string CostCurrency,
    string? ProviderResponseId,
    bool FromCache);
