using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>A specific monetary offer for a <see cref="Plan"/>, mirrors a Stripe Price (ERD ENT-039C).</summary>
public sealed class Price : AuditableEntity<PriceId>
{
    private Price(PriceId id, PlanId planId, Money amount, BillingInterval interval, DateTimeOffset nowUtc)
        : base(id)
    {
        PlanId = planId;
        Amount = amount;
        Interval = interval;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private Price()
    {
        Amount = null!;
    }

    public PlanId PlanId { get; private set; }

    public Money Amount { get; private set; }

    public BillingInterval Interval { get; private set; }

    public bool IsActive { get; private set; }

    public string? StripePriceId { get; private set; }

    public static Price Create(PlanId planId, PriceId id, Money amount, BillingInterval interval, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(amount);
        return new Price(id, planId, amount, interval, nowUtc);
    }

    public void AttachStripePrice(string stripePriceId, DateTimeOffset nowUtc)
    {
        StripePriceId = stripePriceId;
        UpdatedAtUtc = nowUtc;
    }

    public Result Deactivate(DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
