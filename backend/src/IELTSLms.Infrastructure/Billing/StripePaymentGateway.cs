using IELTSLms.Abstractions.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using DomainCoupon = IELTSLms.Domain.Billing.Coupon;

namespace IELTSLms.Infrastructure.Billing;

/// <summary>
/// <see cref="IPaymentGateway"/> implementation over the Stripe.net SDK (13 §10). Retries are handled
/// by Stripe.net's own built-in network-retry support (a native SDK feature) rather than the shared
/// "http-default" resilience pipeline, which targets generic outbound HTTP calls, not a vendor SDK
/// that already retries idempotent requests safely on its own.
/// </summary>
public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly Lazy<StripeClient> _client;

    public StripePaymentGateway(IOptions<StripeOptions> options)
    {
        _client = new Lazy<StripeClient>(() =>
        {
            var secretKey = options.Value.SecretKey;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "Stripe:SecretKey is not configured. Set it via configuration/environment before using billing features.");
            }

            var retryingHttpClient = new SystemNetHttpClient(httpClient: null, maxNetworkRetries: 2);
            return new StripeClient(secretKey, httpClient: retryingHttpClient);
        });
    }

    public async Task<string> EnsureCustomerAsync(
        string? existingCustomerId, string email, string? name, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(existingCustomerId))
        {
            return existingCustomerId;
        }

        var service = new CustomerService(_client.Value);
        var customer = await service.CreateAsync(
            new CustomerCreateOptions { Email = email, Name = name },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return customer.Id;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var service = new SessionService(_client.Value);
        var session = await service.CreateAsync(
            new SessionCreateOptions
            {
                Customer = request.ProviderCustomerId,
                Mode = "subscription",
                LineItems =
                [
                    new SessionLineItemOptions { Price = request.ProviderPriceId, Quantity = 1 },
                ],
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                SubscriptionData = request.TrialPeriodDays > 0
                    ? new SessionSubscriptionDataOptions { TrialPeriodDays = request.TrialPeriodDays }
                    : null,
                Discounts = request.ProviderCouponId is not null
                    ? [new SessionDiscountOptions { Coupon = request.ProviderCouponId }]
                    : null,
                AutomaticTax = request.EnableAutomaticTax
                    ? new SessionAutomaticTaxOptions { Enabled = true }
                    : null,
                Metadata = new Dictionary<string, string>(request.Metadata),
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return session.Url;
    }

    public async Task CancelSubscriptionAsync(
        string providerSubscriptionId, bool atPeriodEnd, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService(_client.Value);

        if (atPeriodEnd)
        {
            await service.UpdateAsync(
                providerSubscriptionId,
                new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await service.CancelAsync(
                providerSubscriptionId, new SubscriptionCancelOptions(), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task ResumeSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService(_client.Value);
        await service.UpdateAsync(
            providerSubscriptionId,
            new SubscriptionUpdateOptions { CancelAtPeriodEnd = false },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> SyncProductAsync(
        string internalCode, string name, string? description, CancellationToken cancellationToken = default)
    {
        var service = new ProductService(_client.Value);
        var product = await service.CreateAsync(
            new ProductCreateOptions
            {
                Name = name,
                Description = description,
                Metadata = new Dictionary<string, string> { ["internal_code"] = internalCode },
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return product.Id;
    }

    public async Task<ProviderSubscriptionSnapshot?> GetSubscriptionAsync(
        string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService(_client.Value);

        try
        {
            var subscription = await service.GetAsync(providerSubscriptionId, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new ProviderSubscriptionSnapshot(
                subscription.Status, subscription.CurrentPeriodStart, subscription.CurrentPeriodEnd);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            return null;
        }
    }

    public async Task<string> SyncPriceAsync(
        string providerProductId, long unitAmountMinorUnits, string currency, BillingInterval interval,
        CancellationToken cancellationToken = default)
    {
        var service = new PriceService(_client.Value);
        var price = await service.CreateAsync(
            new PriceCreateOptions
            {
                Product = providerProductId,
                UnitAmount = unitAmountMinorUnits,
                Currency = currency.ToLowerInvariant(),
                Recurring = new PriceRecurringOptions
                {
                    Interval = interval is BillingInterval.Yearly ? "year" : "month",
                },
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return price.Id;
    }

    // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

    public async Task<string> CreateBillingPortalSessionAsync(
        string providerCustomerId, string returnUrl, CancellationToken cancellationToken = default)
    {
        var service = new Stripe.BillingPortal.SessionService(_client.Value);
        var session = await service.CreateAsync(
            new Stripe.BillingPortal.SessionCreateOptions { Customer = providerCustomerId, ReturnUrl = returnUrl },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return session.Url;
    }

    public async Task<string> CreateSetupIntentAsync(string providerCustomerId, CancellationToken cancellationToken = default)
    {
        var service = new SetupIntentService(_client.Value);
        var setupIntent = await service.CreateAsync(
            new SetupIntentCreateOptions { Customer = providerCustomerId, PaymentMethodTypes = ["card"] },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return setupIntent.ClientSecret;
    }

    public async Task DetachPaymentMethodAsync(string providerPaymentMethodId, CancellationToken cancellationToken = default)
    {
        var service = new PaymentMethodService(_client.Value);
        await service.DetachAsync(providerPaymentMethodId, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task SetDefaultPaymentMethodAsync(
        string providerCustomerId, string providerPaymentMethodId, CancellationToken cancellationToken = default)
    {
        var service = new CustomerService(_client.Value);
        await service.UpdateAsync(
            providerCustomerId,
            new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions { DefaultPaymentMethod = providerPaymentMethodId },
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangeSubscriptionPriceAsync(
        string providerSubscriptionId, string providerPriceId, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService(_client.Value);
        var subscription = await service.GetAsync(providerSubscriptionId, cancellationToken: cancellationToken).ConfigureAwait(false);
        var existingItemId = subscription.Items.Data[0].Id;

        await service.UpdateAsync(
            providerSubscriptionId,
            new SubscriptionUpdateOptions
            {
                Items = [new SubscriptionItemOptions { Id = existingItemId, Price = providerPriceId }],
                ProrationBehavior = "create_prorations",
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> CreateRefundAsync(
        string providerPaymentIntentId, long amountMinorUnits, CancellationToken cancellationToken = default)
    {
        var service = new RefundService(_client.Value);
        var refund = await service.CreateAsync(
            new RefundCreateOptions { PaymentIntent = providerPaymentIntentId, Amount = amountMinorUnits },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return refund.Id;
    }

    public async Task<string> SyncCouponAsync(DomainCoupon coupon, CancellationToken cancellationToken = default)
    {
        var service = new CouponService(_client.Value);

        if (coupon.StripeCouponId is not null)
        {
            return coupon.StripeCouponId;
        }

        var created = await service.CreateAsync(
            new CouponCreateOptions
            {
                Name = coupon.Name,
                Duration = "once",
                PercentOff = coupon.DiscountType is CouponDiscountType.Percentage
                    ? coupon.Value / (decimal)DomainCoupon.BasisPointsPerPercent
                    : null,
                AmountOff = coupon.DiscountType is CouponDiscountType.Fixed ? coupon.Value : null,
                Currency = coupon.DiscountType is CouponDiscountType.Fixed ? coupon.Currency?.ToLowerInvariant() : null,
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return created.Id;
    }

    public async Task RetryInvoicePaymentAsync(string providerInvoiceId, CancellationToken cancellationToken = default)
    {
        var service = new InvoiceService(_client.Value);
        await service.PayAsync(providerInvoiceId, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
