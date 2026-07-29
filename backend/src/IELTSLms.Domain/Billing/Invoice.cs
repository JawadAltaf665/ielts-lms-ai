using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Billing document for a charge period (ERD ENT-041). Immutable once issued.</summary>
public sealed class Invoice : TenantAuditableEntity<InvoiceId>
{
    private Invoice(
        InvoiceId id, Guid tenantId, SubscriptionId subscriptionId, BillingCustomerId billingCustomerId,
        string number, string currency, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        BillingCustomerId = billingCustomerId;
        Number = number;
        Currency = currency;
        Status = InvoiceStatus.Draft;
        CreatedAtUtc = nowUtc;
    }

    private Invoice()
    {
        Number = null!;
        Currency = null!;
    }

    public SubscriptionId SubscriptionId { get; private set; }

    public BillingCustomerId BillingCustomerId { get; private set; }

    public string? StripeInvoiceId { get; private set; }

    public string Number { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public string Currency { get; private set; }

    public long SubtotalMinorUnits { get; private set; }

    public long TaxMinorUnits { get; private set; }

    public long TotalMinorUnits { get; private set; }

    public DateTimeOffset? IssuedAtUtc { get; private set; }

    public DateTimeOffset? DueAtUtc { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    /// <summary>Stripe's hosted invoice page/PDF — "download" reuses this rather than a first-party generator (ADR-032).</summary>
    public string? HostedInvoiceUrl { get; private set; }

    public static Invoice CreateDraft(
        InvoiceId id, Guid tenantId, SubscriptionId subscriptionId, BillingCustomerId billingCustomerId,
        string number, string currency, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        return new Invoice(id, tenantId, subscriptionId, billingCustomerId, number.Trim(), currency.Trim().ToUpperInvariant(), nowUtc);
    }

    public Result Issue(long subtotalMinorUnits, long taxMinorUnits, DateTimeOffset dueAtUtc, DateTimeOffset nowUtc)
    {
        if (Status is not InvoiceStatus.Draft)
        {
            return InvalidTransition("Only a draft invoice can be issued.");
        }

        if (subtotalMinorUnits < 0 || taxMinorUnits < 0)
        {
            return Result.Failure(Error.Validation("VALIDATION_AMOUNT_NEGATIVE", "Invoice amounts cannot be negative."));
        }

        SubtotalMinorUnits = subtotalMinorUnits;
        TaxMinorUnits = taxMinorUnits;
        TotalMinorUnits = subtotalMinorUnits + taxMinorUnits;
        Status = InvoiceStatus.Issued;
        IssuedAtUtc = nowUtc;
        DueAtUtc = dueAtUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public void AttachStripeInvoice(string stripeInvoiceId, DateTimeOffset nowUtc)
    {
        StripeInvoiceId = stripeInvoiceId;
        UpdatedAtUtc = nowUtc;
    }

    public void SetHostedInvoiceUrl(string? hostedInvoiceUrl, DateTimeOffset nowUtc)
    {
        HostedInvoiceUrl = hostedInvoiceUrl;
        UpdatedAtUtc = nowUtc;
    }

    public Result MarkPaid(DateTimeOffset nowUtc)
    {
        if (Status is not InvoiceStatus.Issued)
        {
            return InvalidTransition("Only an issued invoice can be marked paid.");
        }

        Status = InvoiceStatus.Paid;
        PaidAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Void(DateTimeOffset nowUtc)
    {
        if (Status is InvoiceStatus.Paid)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_INVOICE_STATE", "A paid invoice cannot be voided; issue a credit note instead."));
        }

        Status = InvoiceStatus.Void;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result MarkUncollectible(DateTimeOffset nowUtc)
    {
        if (Status is not InvoiceStatus.Issued)
        {
            return InvalidTransition("Only an issued invoice can be marked uncollectible.");
        }

        Status = InvoiceStatus.Uncollectible;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    private static Result InvalidTransition(string message) =>
        Result.Failure(Error.Conflict("CONFLICT_INVOICE_STATE", message));
}
