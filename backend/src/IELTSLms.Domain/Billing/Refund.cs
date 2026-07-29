using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// A full or partial reversal of a <see cref="Payment"/> (ERD ENT-039S, ADR-032). A payment may have
/// several refunds (partial refunds); the sum must never exceed the original payment amount —
/// enforced by the caller before construction (see <c>RefundPaymentCommandHandler</c>), since only
/// the caller can see all of a payment's prior refunds.
/// </summary>
public sealed class Refund : TenantAuditableEntity<RefundId>
{
    private Refund(RefundId id, Guid tenantId, PaymentId paymentId, long amountMinorUnits, string reason, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        PaymentId = paymentId;
        AmountMinorUnits = amountMinorUnits;
        Reason = reason;
        Status = RefundStatus.Pending;
        CreatedAtUtc = nowUtc;
    }

    private Refund()
    {
        Reason = null!;
    }

    public PaymentId PaymentId { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Reason { get; private set; }

    public RefundStatus Status { get; private set; }

    public string? StripeRefundId { get; private set; }

    public static Result<Refund> Create(RefundId id, Guid tenantId, PaymentId paymentId, long amountMinorUnits, string reason, DateTimeOffset nowUtc)
    {
        if (amountMinorUnits <= 0)
        {
            return Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Refund amount must be positive.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Refund reason is required.");
        }

        return new Refund(id, tenantId, paymentId, amountMinorUnits, reason.Trim(), nowUtc);
    }

    public void AttachStripeRefund(string stripeRefundId, DateTimeOffset nowUtc)
    {
        StripeRefundId = stripeRefundId;
        UpdatedAtUtc = nowUtc;
    }

    public Result MarkSucceeded(DateTimeOffset nowUtc)
    {
        if (Status is not RefundStatus.Pending)
        {
            return Result.Failure(Error.Conflict("CONFLICT_REFUND_STATE", "Only a pending refund can succeed."));
        }

        Status = RefundStatus.Succeeded;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result MarkFailed(DateTimeOffset nowUtc)
    {
        if (Status is not RefundStatus.Pending)
        {
            return Result.Failure(Error.Conflict("CONFLICT_REFUND_STATE", "Only a pending refund can fail."));
        }

        Status = RefundStatus.Failed;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
