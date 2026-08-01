using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>Aggregate access for <see cref="AiResponse"/> (ERD ENT-052).</summary>
public interface IAiResponseRepository : IRepository<AiResponse, AiResponseId>
{
    /// <summary>
    /// Tenant-scoped cost/usage aggregation grouped by capability/provider/day (AI Architecture §11.1).
    /// Never accepts a cross-tenant scope — <paramref name="tenantId"/> is a hard filter, not a hint.
    /// </summary>
    Task<IReadOnlyList<AiUsageSummaryRow>> GetUsageSummaryAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default);
}

/// <summary>One (capability, provider, day) bucket of aggregated <see cref="AiResponse"/> rows.</summary>
public sealed record AiUsageSummaryRow(
    string Capability,
    string Provider,
    DateOnly Day,
    int RequestCount,
    long TotalPromptTokens,
    long TotalCompletionTokens,
    long TotalCostMinorUnits,
    string CostCurrency);
