using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.RemovePaymentMethod;

public sealed record RemovePaymentMethodCommand(Guid TenantId, Guid PaymentMethodId) : ICommand;
