using IELTSLms.Domain.Billing;

namespace IELTSLms.Domain.Tests.Billing;

/// <summary>Guards the ADR-032 Pause/Unpause transitions, additive to <see cref="SubscriptionTests"/>.</summary>
public sealed class SubscriptionPauseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Subscription CreateActive()
    {
        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), Guid.NewGuid(), BillingCustomerId.From(Guid.NewGuid()),
            PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        return subscription;
    }

    [Fact]
    public void Pause_from_active_succeeds()
    {
        var subscription = CreateActive();

        var result = subscription.Pause(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Paused, subscription.Status);
    }

    [Fact]
    public void Pause_from_non_active_fails()
    {
        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), Guid.NewGuid(), BillingCustomerId.From(Guid.NewGuid()),
            PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);

        var result = subscription.Pause(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SUBSCRIPTION_STATE", result.Error.Code);
    }

    [Fact]
    public void Unpause_from_paused_succeeds()
    {
        var subscription = CreateActive();
        subscription.Pause(Now);

        var result = subscription.Unpause(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Unpause_from_non_paused_fails()
    {
        var subscription = CreateActive();

        var result = subscription.Unpause(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SUBSCRIPTION_STATE", result.Error.Code);
    }
}
