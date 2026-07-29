using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetSubscription;

namespace IELTSLms.Application.Billing.ChangePlan;

/// <summary>
/// Immediate upgrade/downgrade with Stripe-computed proration (13 §6, ADR-032). Scheduled
/// end-of-period downgrade is available via the Billing Portal; see ADR-032 for why this API is
/// immediate-only.
/// </summary>
public sealed record ChangePlanCommand(Guid TenantId, Guid NewPlanId, Guid NewPriceId) : ICommand<SubscriptionResponse>;
