using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetSubscription;

namespace IELTSLms.Application.Billing.CreateSubscription;

/// <summary>
/// Creates a subscription directly without a Stripe checkout — only valid for the Free plan
/// (13 §2.1: Free has no billing). Paid plans go through <c>CreateCheckoutSessionCommand</c>.
/// </summary>
public sealed record CreateSubscriptionCommand(Guid TenantId, string Email, string? Name, Guid PlanId)
    : ICommand<SubscriptionResponse>;
