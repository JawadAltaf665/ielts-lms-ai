using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.ResumeSubscription;

/// <summary>Reverses a pending cancellation before the current billing period ends (13 §4).</summary>
public sealed record ResumeSubscriptionCommand(Guid TenantId) : ICommand;
