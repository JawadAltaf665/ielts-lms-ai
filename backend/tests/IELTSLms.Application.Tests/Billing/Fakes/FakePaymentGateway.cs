using IELTSLms.Abstractions.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakePaymentGateway : IPaymentGateway
{
    public int CancelCallCount { get; private set; }

    public int ResumeCallCount { get; private set; }

    public Task<string> EnsureCustomerAsync(string? existingCustomerId, string email, string? name, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingCustomerId ?? "cus_fake");

    public Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult("https://checkout.stripe.com/fake");

    public Task CancelSubscriptionAsync(string providerSubscriptionId, bool atPeriodEnd, CancellationToken cancellationToken = default)
    {
        CancelCallCount++;
        return Task.CompletedTask;
    }

    public Task ResumeSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        ResumeCallCount++;
        return Task.CompletedTask;
    }

    public Task<string> SyncProductAsync(string internalCode, string name, string? description, CancellationToken cancellationToken = default) =>
        Task.FromResult("prod_fake");

    public Task<string> SyncPriceAsync(
        string providerProductId, long unitAmountMinorUnits, string currency, BillingInterval interval, CancellationToken cancellationToken = default) =>
        Task.FromResult("price_fake");

    public Task<ProviderSubscriptionSnapshot?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<ProviderSubscriptionSnapshot?>(null);

    // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

    public int ChangeSubscriptionPriceCallCount { get; private set; }

    public int RefundCallCount { get; private set; }

    public Task<string> CreateBillingPortalSessionAsync(string providerCustomerId, string returnUrl, CancellationToken cancellationToken = default) =>
        Task.FromResult("https://billing.stripe.com/fake");

    public Task<string> CreateSetupIntentAsync(string providerCustomerId, CancellationToken cancellationToken = default) =>
        Task.FromResult("seti_fake_secret");

    public Task DetachPaymentMethodAsync(string providerPaymentMethodId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetDefaultPaymentMethodAsync(string providerCustomerId, string providerPaymentMethodId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task ChangeSubscriptionPriceAsync(string providerSubscriptionId, string providerPriceId, CancellationToken cancellationToken = default)
    {
        ChangeSubscriptionPriceCallCount++;
        return Task.CompletedTask;
    }

    public Task<string> CreateRefundAsync(string providerPaymentIntentId, long amountMinorUnits, CancellationToken cancellationToken = default)
    {
        RefundCallCount++;
        return Task.FromResult("re_fake");
    }

    public Task<string> SyncCouponAsync(Coupon coupon, CancellationToken cancellationToken = default) =>
        Task.FromResult("coupon_fake");

    public Task RetryInvoicePaymentAsync(string providerInvoiceId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
