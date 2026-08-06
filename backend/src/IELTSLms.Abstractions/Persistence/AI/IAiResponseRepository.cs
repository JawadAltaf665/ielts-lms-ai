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

    /// <summary>
    /// Time-series cost/usage trend grouped by capability/provider/day (Sprint 10 analytics).
    /// Same per-day grain as <see cref="GetUsageSummaryAsync"/>, exposed at the analytics namespace
    /// with a "trend" framing for the <c>GET /api/v1/analytics/ai/usage-trends</c> endpoint.
    /// </summary>
    Task<IReadOnlyList<AiUsageSummaryRow>> GetUsageTrendsAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Per-capability per-day quality signals: failure rate (status=Failed responses / total),
    /// escalation/HITL rate (RequiresReview flag where present). Derived from fields already
    /// recorded since Sprint 1 — no new AI calls or schema changes required.
    /// </summary>
    Task<IReadOnlyList<AiQualityTrendRow>> GetQualityTrendsAsync(
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

/// <summary>
/// Per-capability per-day quality signal row (Sprint 10, <c>GET /api/v1/analytics/ai/quality-trends</c>).
/// All rates are in the [0, 1] range; null when no requests exist for that day/capability.
/// </summary>
public sealed record AiQualityTrendRow(
    string Capability,
    DateOnly Day,
    int TotalRequests,
    int FailedRequests,
    /// <summary>Fraction of requests that failed (Failed / Total). Null when Total == 0.</summary>
    decimal? FailureRate,
    int EscalatedRequests,
    /// <summary>Fraction of requests flagged for human review (Escalated / Total). Null when Total == 0.</summary>
    decimal? EscalationRate);
