using IELTSLms.Application.Billing.ChangePlan;
using IELTSLms.Application.Tests.Billing.Fakes;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing;

public sealed class ChangePlanCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Plan CreatePlan(int displayOrder, PlanCode code) =>
        Plan.Create(PlanId.From(Guid.NewGuid()), ProductId.From(Guid.NewGuid()), code, code.ToString(), displayOrder, false, 0, Now).Value;

    private static Price CreatePrice(PlanId planId, string? stripePriceId = "price_new")
    {
        var price = Price.Create(planId, PriceId.From(Guid.NewGuid()), Money.Create(1000, "usd").Value, BillingInterval.Monthly, Now);
        if (stripePriceId is not null)
        {
            price.AttachStripePrice(stripePriceId, Now);
        }

        return price;
    }

    [Fact]
    public async Task ChangePlan_to_higher_tier_succeeds_and_calls_gateway()
    {
        var tenantId = Guid.NewGuid();
        var oldPlan = CreatePlan(1, PlanCode.Starter);
        var newPlan = CreatePlan(2, PlanCode.Premium);
        var newPrice = CreatePrice(newPlan.Id);

        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), tenantId, BillingCustomerId.From(Guid.NewGuid()), oldPlan.Id, PriceId.From(Guid.NewGuid()), Now);
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var subscriptions = new FakeSubscriptionRepository();
        subscriptions.Seed(subscription);
        var plans = new FakePlanRepository();
        plans.Seed(oldPlan);
        plans.Seed(newPlan);
        var prices = new FakePriceRepository();
        prices.Seed(newPrice);
        var gateway = new FakePaymentGateway();

        var handler = new ChangePlanCommandHandler(
            subscriptions, plans, prices, new FakeBillingCustomerRepository(), gateway, new FakeEmailSender(),
            new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new ChangePlanCommand(tenantId, newPlan.Id.Value, newPrice.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPlan.Id, subscription.PlanId);
        Assert.Equal(1, gateway.ChangeSubscriptionPriceCallCount);
    }

    [Fact]
    public async Task ChangePlan_with_unsynced_price_fails()
    {
        var tenantId = Guid.NewGuid();
        var oldPlan = CreatePlan(1, PlanCode.Starter);
        var newPlan = CreatePlan(2, PlanCode.Premium);
        var newPrice = CreatePrice(newPlan.Id, stripePriceId: null);

        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), tenantId, BillingCustomerId.From(Guid.NewGuid()), oldPlan.Id, PriceId.From(Guid.NewGuid()), Now);
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var subscriptions = new FakeSubscriptionRepository();
        subscriptions.Seed(subscription);
        var plans = new FakePlanRepository();
        plans.Seed(oldPlan);
        plans.Seed(newPlan);
        var prices = new FakePriceRepository();
        prices.Seed(newPrice);

        var handler = new ChangePlanCommandHandler(
            subscriptions, plans, prices, new FakeBillingCustomerRepository(), new FakePaymentGateway(), new FakeEmailSender(),
            new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new ChangePlanCommand(tenantId, newPlan.Id.Value, newPrice.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_PRICE_INACTIVE", result.Error.Code);
    }

    [Fact]
    public async Task ChangePlan_without_subscription_fails()
    {
        var handler = new ChangePlanCommandHandler(
            new FakeSubscriptionRepository(), new FakePlanRepository(), new FakePriceRepository(), new FakeBillingCustomerRepository(),
            new FakePaymentGateway(), new FakeEmailSender(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new ChangePlanCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_SUBSCRIPTION_NOT_FOUND", result.Error.Code);
    }
}
