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
}
