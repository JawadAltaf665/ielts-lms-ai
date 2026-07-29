using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Record of a payment attempt/settlement against an <see cref="Invoice"/> (ERD ENT-042).
/// No raw card data is stored — only the provider's payment-intent reference.
/// </summary>
public sealed class Payment : TenantAuditableEntity<PaymentId>
{
    private Payment(PaymentId id, Guid tenantId, InvoiceId invoiceId, long amountMinorUnits, string currency, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        InvoiceId = invoiceId;
        AmountMinorUnits = amountMinorUnits;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = nowUtc;
    }

    private Payment()
    {
        Currency = null!;
    }

    public InvoiceId InvoiceId { get; private set; }

    public string? StripePaymentIntentId { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Currency { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public static Result<Payment> Create(
        PaymentId id, Guid tenantId, InvoiceId invoiceId, long amountMinorUnits, string currency, DateTimeOffset nowUtc)
    {
        if (amountMinorUnits < 0)
        {
            return Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Payment amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Payment currency is required.");
        }

        return new Payment(id, tenantId, invoiceId, amountMinorUnits, currency.Trim().ToUpperInvariant(), nowUtc);
    }

    public void AttachStripePaymentIntent(string stripePaymentIntentId, DateTimeOffset nowUtc)
    {
        StripePaymentIntentId = stripePaymentIntentId;
        UpdatedAtUtc = nowUtc;
    }

    public Result MarkSucceeded(DateTimeOffset nowUtc)
    {
        if (Status is not PaymentStatus.Pending)
        {
            return InvalidTransition("Only a pending payment can succeed.");
        }

        Status = PaymentStatus.Succeeded;
        PaidAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result MarkFailed(string reason, DateTimeOffset nowUtc)
    {
        if (Status is not PaymentStatus.Pending)
        {
            return InvalidTransition("Only a pending payment can fail.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Refund(DateTimeOffset nowUtc)
    {
        if (Status is not PaymentStatus.Succeeded)
        {
            return InvalidTransition("Only a succeeded payment can be refunded.");
        }

        Status = PaymentStatus.Refunded;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    private static Result InvalidTransition(string message) =>
        Result.Failure(Error.Conflict("CONFLICT_PAYMENT_STATE", message));
}
