using IELTSLms.Application.Billing.CancelSubscription;
using IELTSLms.Application.Tests.Billing.Fakes;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing;

public sealed class CancelSubscriptionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Cancels_active_subscription_and_calls_gateway_when_stripe_linked()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), tenantId, BillingCustomerId.From(Guid.NewGuid()),
            PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var subscriptions = new FakeSubscriptionRepository();
        subscriptions.Seed(subscription);
        var gateway = new FakePaymentGateway();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelSubscriptionCommandHandler(subscriptions, gateway, unitOfWork, new FakeClock(Now));

        var result = await handler.Handle(new CancelSubscriptionCommand(tenantId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal(1, gateway.CancelCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Returns_not_found_when_tenant_has_no_subscription()
    {
        var handler = new CancelSubscriptionCommandHandler(
            new FakeSubscriptionRepository(), new FakePaymentGateway(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new CancelSubscriptionCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_SUBSCRIPTION_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Returns_conflict_when_subscription_already_pending()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), tenantId, BillingCustomerId.From(Guid.NewGuid()),
            PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);

        var subscriptions = new FakeSubscriptionRepository();
        subscriptions.Seed(subscription);
        var handler = new CancelSubscriptionCommandHandler(
            subscriptions, new FakePaymentGateway(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new CancelSubscriptionCommand(tenantId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SUBSCRIPTION_STATE", result.Error.Code);
    }
}
