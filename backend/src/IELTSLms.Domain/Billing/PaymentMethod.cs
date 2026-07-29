using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// A reusable tokenized payment method, mirrors a Stripe PaymentMethod (ERD ENT-039M).
/// No raw card data is stored — only the provider token and display-only card metadata.
/// </summary>
public sealed class PaymentMethod : TenantAuditableEntity<PaymentMethodId>
{
    private PaymentMethod(
        PaymentMethodId id, Guid tenantId, BillingCustomerId billingCustomerId, string stripePaymentMethodId,
        string brand, string last4, int expMonth, int expYear, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        BillingCustomerId = billingCustomerId;
        StripePaymentMethodId = stripePaymentMethodId;
        Brand = brand;
        Last4 = last4;
        ExpMonth = expMonth;
        ExpYear = expYear;
        CreatedAtUtc = nowUtc;
    }

    private PaymentMethod()
    {
        StripePaymentMethodId = null!;
        Brand = null!;
        Last4 = null!;
    }

    public BillingCustomerId BillingCustomerId { get; private set; }

    public string StripePaymentMethodId { get; private set; }

    public string Brand { get; private set; }

    public string Last4 { get; private set; }

    public int ExpMonth { get; private set; }

    public int ExpYear { get; private set; }

    public bool IsDefault { get; private set; }

    public static Result<PaymentMethod> Create(
        PaymentMethodId id, Guid tenantId, BillingCustomerId billingCustomerId, string stripePaymentMethodId,
        string brand, string last4, int expMonth, int expYear, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(stripePaymentMethodId))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Stripe payment method id is required.");
        }

        return new PaymentMethod(id, tenantId, billingCustomerId, stripePaymentMethodId, brand, last4, expMonth, expYear, nowUtc);
    }

    public void MakeDefault(DateTimeOffset nowUtc)
    {
        IsDefault = true;
        UpdatedAtUtc = nowUtc;
    }

    public void ClearDefault(DateTimeOffset nowUtc)
    {
        IsDefault = false;
        UpdatedAtUtc = nowUtc;
    }
}
