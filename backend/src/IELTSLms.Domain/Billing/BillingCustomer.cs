using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Tenant's billing identity, mirrors a Stripe Customer (ERD ENT-039L). One per tenant.</summary>
public sealed class BillingCustomer : TenantAuditableEntity<BillingCustomerId>
{
    private BillingCustomer(BillingCustomerId id, Guid tenantId, string email, string? name, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        Name = name;
        CreatedAtUtc = nowUtc;
    }

    private BillingCustomer()
    {
        Email = null!;
    }

    public string Email { get; private set; }

    public string? Name { get; private set; }

    public string? Phone { get; private set; }

    public string? StripeCustomerId { get; private set; }

    /// <summary>Owned value object — see ADR-031 for why this isn't its own aggregate.</summary>
    public BillingAddress? Address { get; private set; }

    public static Result<BillingCustomer> Create(BillingCustomerId id, Guid tenantId, string email, string? name, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Billing customer email is required.");
        }

        return new BillingCustomer(id, tenantId, email.Trim(), name?.Trim(), nowUtc);
    }

    public void AttachStripeCustomer(string stripeCustomerId, DateTimeOffset nowUtc)
    {
        StripeCustomerId = stripeCustomerId;
        UpdatedAtUtc = nowUtc;
    }

    public Result UpdateProfile(string email, string? name, string? phone, BillingAddress? address, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(Error.Validation("VALIDATION_REQUIRED", "Billing customer email is required."));
        }

        Email = email.Trim();
        Name = name?.Trim();
        Phone = phone?.Trim();
        Address = address;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
