using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.GetAiUsageTrends;

/// <summary>
/// Retrieves a time-series view of AI usage and costs per capability for the specified date range.
/// Used by the analytics dashboard (Sprint 10).
/// </summary>
public sealed record GetAiUsageTrendsQuery(Guid TenantId, DateTimeOffset FromUtc, DateTimeOffset ToUtc)
    : IQuery<IReadOnlyList<AiUsageSummaryRow>>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["analytics.read"];
}
