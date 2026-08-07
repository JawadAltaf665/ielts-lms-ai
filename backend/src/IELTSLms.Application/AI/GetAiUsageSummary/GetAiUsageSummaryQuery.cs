using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.GetAiUsageSummary;

/// <summary>
/// Tenant-scoped AI cost/usage aggregation by capability/provider/day (AI Architecture §11.1). Budget
/// governance, not general cohort analytics — scoped to Tenant Admin/Institute Admin, narrower than
/// <c>analytics.view.cohort</c>'s Teacher access. <see cref="From"/>/<see cref="To"/> default to the
/// trailing 30 days when omitted.
/// </summary>
public sealed record GetAiUsageSummaryQuery(Guid TenantId, DateOnly? From, DateOnly? To)
    : IQuery<AiUsageSummaryResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Institute Admin"];
}

public sealed record AiUsageSummaryResponse(IReadOnlyList<AiUsageSummaryItemResponse> Items);

public sealed record AiUsageSummaryItemResponse(
    string Capability,
    string Provider,
    DateOnly Day,
    int RequestCount,
    long TotalPromptTokens,
    long TotalCompletionTokens,
    long TotalCostMinorUnits,
    string CostCurrency);
