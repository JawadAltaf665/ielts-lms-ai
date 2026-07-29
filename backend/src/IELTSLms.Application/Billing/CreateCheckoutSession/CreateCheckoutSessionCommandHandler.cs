using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.CreateCheckoutSession;

public sealed class CreateCheckoutSessionCommandHandler(
    IPlanRepository plans,
    IPriceRepository prices,
    IProductRepository products,
    IBillingCustomerRepository billingCustomers,
    ISubscriptionRepository subscriptions,
    ISubscriptionItemRepository subscriptionItems,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids)
    : ICommandHandler<CreateCheckoutSessionCommand, CheckoutSessionResponse>
{
    public async Task<Result<CheckoutSessionResponse>> Handle(
        CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var plan = await plans.GetByIdAsync(PlanId.From(request.PlanId), cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.PlanNotFound);
        }

        if (!plan.IsActive)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.PlanInactive);
        }

        var price = await prices.GetByIdAsync(PriceId.From(request.PriceId), cancellationToken).ConfigureAwait(false);
        if (price is null)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.PriceNotFound);
        }

        if (price.PlanId != plan.Id)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.PriceDoesNotBelongToPlan);
        }

        if (!price.IsActive)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.PriceInactive);
        }

        var existingSubscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (existingSubscription is not null)
        {
            return Result.Failure<CheckoutSessionResponse>(BillingErrors.SubscriptionAlreadyExists);
        }

        var now = clock.UtcNow;

        var billingCustomer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (billingCustomer is null)
        {
            var createResult = BillingCustomer.Create(
                BillingCustomerId.From(guids.NewGuid()), request.TenantId, request.Email, request.Name, now);
            if (createResult.IsFailure)
            {
                return Result.Failure<CheckoutSessionResponse>(createResult.Error);
            }

            billingCustomer = createResult.Value;
            billingCustomers.Add(billingCustomer);
        }

        await EnsureStripeCatalogSyncedAsync(plan, price, cancellationToken).ConfigureAwait(false);

        var stripeCustomerId = await paymentGateway
            .EnsureCustomerAsync(billingCustomer.StripeCustomerId, request.Email, request.Name, cancellationToken)
            .ConfigureAwait(false);
        billingCustomer.AttachStripeCustomer(stripeCustomerId, now);

        var subscription = Subscription.Create(
            SubscriptionId.From(guids.NewGuid()), request.TenantId, billingCustomer.Id, plan.Id, price.Id, now);
        subscriptions.Add(subscription);

        var itemResult = SubscriptionItem.Create(
            SubscriptionItemId.From(guids.NewGuid()), subscription.Id, price.Id, quantity: 1, now);
        if (itemResult.IsSuccess)
        {
            subscriptionItems.Add(itemResult.Value);
        }

        var checkoutUrl = await paymentGateway.CreateCheckoutSessionAsync(
            new CreateCheckoutSessionRequest(
                stripeCustomerId,
                price.StripePriceId ?? throw new InvalidOperationException("Price was not synced to Stripe."),
                request.SuccessUrl,
                request.CancelUrl,
                plan.TrialEligible ? plan.TrialDays : 0,
                new Dictionary<string, string>
                {
                    ["subscription_id"] = subscription.Id.Value.ToString(),
                    ["tenant_id"] = request.TenantId.ToString(),
                }),
            cancellationToken).ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new CheckoutSessionResponse(checkoutUrl));
    }

    /// <summary>Lazily syncs the plan's product/price to Stripe on first use rather than requiring a separate admin sync step.</summary>
    private async Task EnsureStripeCatalogSyncedAsync(Plan plan, Price price, CancellationToken cancellationToken)
    {
        if (price.StripePriceId is not null)
        {
            return;
        }

        // The product must exist in Stripe before a price can reference it. Products are synced by
        // the same lazy path the first time any of their plan's prices is used in checkout.
        var product = await products.GetByIdAsync(plan.ProductId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Plan {plan.Id} references a missing product {plan.ProductId}.");

        if (product.StripeProductId is null)
        {
            var stripeProductId = await paymentGateway
                .SyncProductAsync(product.InternalCode, product.Name, product.Description, cancellationToken)
                .ConfigureAwait(false);
            product.AttachStripeProduct(stripeProductId, clock.UtcNow);
        }

        var stripePriceId = await paymentGateway
            .SyncPriceAsync(product.StripeProductId!, price.Amount.AmountMinorUnits, price.Amount.Currency, price.Interval, cancellationToken)
            .ConfigureAwait(false);
        price.AttachStripePrice(stripePriceId, clock.UtcNow);
    }
}
