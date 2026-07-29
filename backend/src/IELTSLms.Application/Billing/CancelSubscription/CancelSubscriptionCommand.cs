using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.CancelSubscription;

/// <summary>Cancels the tenant's subscription; access continues until the current period ends (13 §4).</summary>
public sealed record CancelSubscriptionCommand(Guid TenantId) : ICommand;
