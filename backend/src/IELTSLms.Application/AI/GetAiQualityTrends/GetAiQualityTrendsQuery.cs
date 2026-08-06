using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.GetAiQualityTrends;

/// <summary>
/// Retrieves a time-series view of AI quality signals (failure rate, escalation rate) per capability
/// for the specified date range. Used by the analytics dashboard (Sprint 10).
/// </summary>
public sealed record GetAiQualityTrendsQuery(Guid TenantId, DateTimeOffset FromUtc, DateTimeOffset ToUtc)
    : IQuery<IReadOnlyList<AiQualityTrendRow>>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["analytics.read"];
}
