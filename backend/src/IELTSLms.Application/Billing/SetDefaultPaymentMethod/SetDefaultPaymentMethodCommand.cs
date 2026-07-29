using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.SetDefaultPaymentMethod;

public sealed record SetDefaultPaymentMethodCommand(Guid TenantId, Guid PaymentMethodId) : ICommand;
