using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.RetryPayment;

/// <summary>Re-attempts a failed payment via Stripe's invoice pay (dunning recovery, 13 §12).</summary>
public sealed record RetryPaymentCommand(Guid TenantId, Guid PaymentId) : ICommand;
