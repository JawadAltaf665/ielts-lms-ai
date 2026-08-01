using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Caching;
using IELTSLms.Infrastructure.AI.Safety;
using IELTSLms.SharedKernel.Results;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace IELTSLms.Infrastructure.AI;

/// <summary>
/// <see cref="IAiGateway"/> implementation (AI Architecture §4): safety/PII guard → response cache →
/// provider call (behind the resilience pipeline configured on the injected <see cref="IAiChatProvider"/>'s
/// <c>HttpClient</c>) → this is the only Infrastructure-layer piece of the gateway; it never touches a
/// DbContext — the caller persists the outcome as an <c>AiResponse</c>.
/// </summary>
public sealed class AiGateway(IEnumerable<IAiChatProvider> chatProviders, ICacheService cache) : IAiGateway
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public async Task<Result<AiGatewayInvocationResult>> InvokeAsync(
        AiGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var chatProvider = chatProviders.FirstOrDefault(p => p.Kind == request.ProviderKind);
        if (chatProvider is null)
        {
            return Result.Failure<AiGatewayInvocationResult>(
                Error.ServiceUnavailable(
                    "DEPENDENCY_AI_PROVIDER_NOT_CONFIGURED", "The configured AI provider is not available."));
        }

        var safety = PromptSafetyGuard.Evaluate(request.UserInput);
        if (safety.IsBlocked)
        {
            return Result.Failure<AiGatewayInvocationResult>(
                Error.Validation("VALIDATION_AI_INPUT_REJECTED", "The submitted text could not be processed."));
        }

        var inputHash = ComputeHash(safety.SanitizedText);
        var cacheKey = $"ai:{request.TenantId}:{request.Feature}:{request.PromptVersion}:{inputHash}";

        var cached = await cache.GetAsync<CachedAiResult>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return Result.Success(new AiGatewayInvocationResult(
                cached.OutputJson, cached.Model, PromptTokens: 0, CompletionTokens: 0, LatencyMs: 0,
                CostMinorUnits: 0, cached.CostCurrency, cached.ProviderResponseId, FromCache: true));
        }

        var stopwatch = Stopwatch.StartNew();
        AiChatResult chatResult;
        try
        {
            chatResult = await chatProvider.CompleteAsync(
                new AiChatRequest(
                    request.SystemPrompt, safety.SanitizedText, request.ResponseSchemaName, request.ResponseSchemaJson,
                    request.MaxOutputTokens),
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or BrokenCircuitException or TimeoutRejectedException or TaskCanceledException)
        {
            return Result.Failure<AiGatewayInvocationResult>(
                Error.ServiceUnavailable("DEPENDENCY_AI_UNAVAILABLE", "The AI provider is temporarily unavailable."));
        }

        stopwatch.Stop();

        if (!IsWellFormedJson(chatResult.StructuredJson))
        {
            return Result.Failure<AiGatewayInvocationResult>(
                Error.Validation("VALIDATION_AI_OUTPUT_INVALID", "The AI response was not valid structured output."));
        }

        var result = new AiGatewayInvocationResult(
            chatResult.StructuredJson, chatResult.Model, chatResult.PromptTokens, chatResult.CompletionTokens,
            (int)stopwatch.ElapsedMilliseconds, chatResult.CostMinorUnits, chatResult.CostCurrency,
            chatResult.ProviderResponseId, FromCache: false);

        await cache.SetAsync(
            cacheKey,
            new CachedAiResult(result.OutputJson, result.Model, result.CostCurrency, result.ProviderResponseId),
            new CacheEntryOptions { AbsoluteExpiration = CacheTtl },
            cancellationToken).ConfigureAwait(false);

        return Result.Success(result);
    }

    private static bool IsWellFormedJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private sealed record CachedAiResult(string OutputJson, string Model, string CostCurrency, string? ProviderResponseId);
}
