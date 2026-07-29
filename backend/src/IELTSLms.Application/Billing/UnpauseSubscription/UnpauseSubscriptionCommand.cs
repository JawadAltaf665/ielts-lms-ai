using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.UnpauseSubscription;

public sealed record UnpauseSubscriptionCommand(Guid TenantId) : ICommand;
