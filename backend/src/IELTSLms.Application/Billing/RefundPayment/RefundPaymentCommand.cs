using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.RefundPayment;

/// <summary>Full or partial refund against a succeeded payment (ERD ENT-039S, ADR-032, Sprint 2 §14).</summary>
public sealed record RefundPaymentCommand(Guid TenantId, Guid PaymentId, long AmountMinorUnits, string Reason) : ICommand<RefundResponse>;

public sealed record RefundResponse(Guid Id, long AmountMinorUnits, string Status);
