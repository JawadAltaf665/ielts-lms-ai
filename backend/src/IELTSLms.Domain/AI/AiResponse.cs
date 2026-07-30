using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A recorded AI provider call, written exactly once per gateway invocation for traceability and cost
/// governance (ERD ENT-052, AI Architecture §11–12). Unlike <see cref="AiProvider"/>/<see cref="AiPrompt"/>
/// this is a real per-tenant transactional log row, so it extends <see cref="TenantAuditableEntity{TId}"/>
/// and gets the automatic tenant query filter. Write-once: there is no mutation method, only the
/// <see cref="Create"/> factory — a cache hit reuses an existing row's id rather than writing a new one.
/// </summary>
public sealed class AiResponse : TenantAuditableEntity<AiResponseId>
{
    private AiResponse(
        AiResponseId id, Guid tenantId, AiProviderId aiProviderId, AiPromptId? aiPromptId, int promptVersion,
        string requestingEntityType, Guid requestingEntityId, string inputHash, string model, int promptTokens,
        int completionTokens, int latencyMs, long costMinorUnits, string costCurrency, AiResponseStatus status,
        string? outputJson, string? providerResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        AiProviderId = aiProviderId;
        AiPromptId = aiPromptId;
        PromptVersion = promptVersion;
        RequestingEntityType = requestingEntityType;
        RequestingEntityId = requestingEntityId;
        InputHash = inputHash;
        Model = model;
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
        LatencyMs = latencyMs;
        CostMinorUnits = costMinorUnits;
        CostCurrency = costCurrency;
        Status = status;
        OutputJson = outputJson;
        ProviderResponseId = providerResponseId;
        CreatedAtUtc = nowUtc;
    }

    private AiResponse()
    {
        RequestingEntityType = null!;
        InputHash = null!;
        Model = null!;
        CostCurrency = null!;
    }

    public AiProviderId AiProviderId { get; private init; }

    public AiPromptId? AiPromptId { get; private init; }

    /// <summary>Denormalized from the prompt used, retained even if that prompt is later retired.</summary>
    public int PromptVersion { get; private init; }

    /// <summary>Soft reference (type + id) to the entity that triggered this call, e.g. <c>"AiOperation"</c>.</summary>
    public string RequestingEntityType { get; private init; }

    public Guid RequestingEntityId { get; private init; }

    /// <summary>Hash of the normalized input, used as part of the response cache key.</summary>
    public string InputHash { get; private init; }

    public string Model { get; private init; }

    public int PromptTokens { get; private init; }

    public int CompletionTokens { get; private init; }

    public int LatencyMs { get; private init; }

    public long CostMinorUnits { get; private init; }

    public string CostCurrency { get; private init; }

    public AiResponseStatus Status { get; private init; }

    /// <summary>Schema-validated structured output; <see langword="null"/> when <see cref="Status"/> is not <see cref="AiResponseStatus.Succeeded"/>.</summary>
    public string? OutputJson { get; private init; }

    public string? ProviderResponseId { get; private init; }

    public static Result<AiResponse> Create(
        AiResponseId id, Guid tenantId, AiProviderId aiProviderId, AiPromptId? aiPromptId, int promptVersion,
        string requestingEntityType, Guid requestingEntityId, string inputHash, string model, int promptTokens,
        int completionTokens, int latencyMs, long costMinorUnits, string costCurrency, AiResponseStatus status,
        string? outputJson, string? providerResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(inputHash))
        {
            return Result.Failure<AiResponse>(AiDomainErrors.ResponseInputHashRequired);
        }

        return Result.Success(new AiResponse(
            id, tenantId, aiProviderId, aiPromptId, promptVersion, requestingEntityType, requestingEntityId,
            inputHash, model, promptTokens, completionTokens, latencyMs, costMinorUnits, costCurrency, status,
            outputJson, providerResponseId, nowUtc));
    }
}
