using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetPlans;

/// <summary>Lists active subscription plans with their prices — the public plan catalog (13 §2.1).</summary>
public sealed record GetPlansQuery : IQuery<IReadOnlyList<PlanSummaryResponse>>;

public sealed record PlanSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    int DisplayOrder,
    bool TrialEligible,
    int TrialDays,
    IReadOnlyList<PriceResponse> Prices);

public sealed record PriceResponse(
    Guid Id,
    Guid PlanId,
    string Currency,
    long UnitAmountMinorUnits,
    string Interval);
