using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Line item of a <see cref="Subscription"/>, supports seat-based quantity (ERD ENT-039N).</summary>
public sealed class SubscriptionItem : AuditableEntity<SubscriptionItemId>
{
    private SubscriptionItem(SubscriptionItemId id, SubscriptionId subscriptionId, PriceId priceId, int quantity, DateTimeOffset nowUtc)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        PriceId = priceId;
        Quantity = quantity;
        CreatedAtUtc = nowUtc;
    }

    private SubscriptionItem()
    {
    }

    public SubscriptionId SubscriptionId { get; private set; }

    public PriceId PriceId { get; private set; }

    public int Quantity { get; private set; }

    public string? StripeSubscriptionItemId { get; private set; }

    public static Result<SubscriptionItem> Create(
        SubscriptionItemId id, SubscriptionId subscriptionId, PriceId priceId, int quantity, DateTimeOffset nowUtc)
    {
        if (quantity < 1)
        {
            return Error.Validation("VALIDATION_QUANTITY", "Quantity must be at least 1.");
        }

        return new SubscriptionItem(id, subscriptionId, priceId, quantity, nowUtc);
    }

    public void AttachStripeItem(string stripeSubscriptionItemId, DateTimeOffset nowUtc)
    {
        StripeSubscriptionItemId = stripeSubscriptionItemId;
        UpdatedAtUtc = nowUtc;
    }

    public Result UpdateQuantity(int quantity, DateTimeOffset nowUtc)
    {
        if (quantity < 1)
        {
            return Result.Failure(Error.Validation("VALIDATION_QUANTITY", "Quantity must be at least 1."));
        }

        Quantity = quantity;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
