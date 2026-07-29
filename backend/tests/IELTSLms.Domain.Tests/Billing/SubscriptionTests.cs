using IELTSLms.Domain.Billing;

namespace IELTSLms.Domain.Tests.Billing;

/// <summary>
/// Guards the <see cref="Subscription"/> state machine (13 §4, ADR-031): every legal transition
/// succeeds and raises its event, every illegal one fails with a conflict and mutates nothing.
/// </summary>
public sealed class SubscriptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Subscription CreatePending() => Subscription.Create(
        SubscriptionId.From(Guid.NewGuid()), Guid.NewGuid(), BillingCustomerId.From(Guid.NewGuid()),
        PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);

    [Fact]
    public void Create_starts_pending_and_raises_created_event()
    {
        var subscription = CreatePending();

        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.Single(subscription.DomainEvents);
    }

    [Fact]
    public void StartTrial_from_pending_succeeds_and_sets_trial_end()
    {
        var subscription = CreatePending();
        var trialEnd = Now.AddDays(14);

        var result = subscription.StartTrial(trialEnd, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, subscription.Status);
        Assert.Equal(trialEnd, subscription.TrialEndsAtUtc);
    }

    [Fact]
    public void StartTrial_from_non_pending_fails()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var result = subscription.StartTrial(Now.AddDays(14), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SUBSCRIPTION_STATE", result.Error.Code);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Pending)]
    [InlineData(SubscriptionStatus.Trial)]
    [InlineData(SubscriptionStatus.PastDue)]
    public void Activate_from_valid_states_succeeds(SubscriptionStatus fromStatus)
    {
        var subscription = CreatePending();
        MoveTo(subscription, fromStatus);

        var result = subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Activate_from_trial_raises_trial_ended_and_activated_events()
    {
        var subscription = CreatePending();
        subscription.StartTrial(Now.AddDays(14), Now);

        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        Assert.Contains(subscription.DomainEvents, e => e.GetType().Name == "TrialEndedDomainEvent");
        Assert.Contains(subscription.DomainEvents, e => e.GetType().Name == "SubscriptionActivatedDomainEvent");
    }

    [Fact]
    public void Activate_from_checkout_raises_checkout_completed_event()
    {
        var subscription = CreatePending();

        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now, fromCheckout: true);

        Assert.Contains(subscription.DomainEvents, e => e.GetType().Name == "CheckoutCompletedDomainEvent");
    }

    [Fact]
    public void Activate_from_active_fails()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var result = subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Renew_from_active_extends_period()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        var newEnd = Now.AddMonths(2);

        var result = subscription.Renew(Now.AddMonths(1), newEnd, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(newEnd, subscription.CurrentPeriodEnd);
    }

    [Fact]
    public void Renew_from_non_active_fails()
    {
        var subscription = CreatePending();

        var result = subscription.Renew(Now, Now.AddMonths(1), Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MarkPastDue_from_active_succeeds()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var result = subscription.MarkPastDue(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.PastDue, subscription.Status);
    }

    [Fact]
    public void Suspend_from_past_due_succeeds()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        subscription.MarkPastDue(Now);

        var result = subscription.Suspend(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Suspended, subscription.Status);
    }

    [Fact]
    public void Suspend_from_active_fails()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);

        var result = subscription.Suspend(Now);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trial)]
    [InlineData(SubscriptionStatus.Active)]
    [InlineData(SubscriptionStatus.PastDue)]
    public void Cancel_from_valid_states_succeeds_and_sets_cancel_at_period_end(SubscriptionStatus fromStatus)
    {
        var subscription = CreatePending();
        MoveTo(subscription, fromStatus);

        var result = subscription.Cancel(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.True(subscription.CancelAtPeriodEnd);
        Assert.Contains(subscription.DomainEvents, e => e.GetType().Name == "SubscriptionCancelledDomainEvent");
    }

    [Fact]
    public void Cancel_from_pending_fails()
    {
        var subscription = CreatePending();

        var result = subscription.Cancel(Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Resume_from_cancelled_before_period_end_succeeds()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        subscription.Cancel(Now);

        var result = subscription.Resume(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.False(subscription.CancelAtPeriodEnd);
    }

    [Fact]
    public void Resume_after_period_end_fails()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        subscription.Cancel(Now);

        var result = subscription.Resume(Now.AddMonths(2));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SUBSCRIPTION_PERIOD_ENDED", result.Error.Code);
    }

    [Fact]
    public void Resume_from_non_cancelled_fails()
    {
        var subscription = CreatePending();

        var result = subscription.Resume(Now);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trial)]
    [InlineData(SubscriptionStatus.PastDue)]
    [InlineData(SubscriptionStatus.Suspended)]
    public void Expire_from_valid_states_succeeds(SubscriptionStatus fromStatus)
    {
        var subscription = CreatePending();
        MoveTo(subscription, fromStatus);

        var result = subscription.Expire(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Expired, subscription.Status);
    }

    [Fact]
    public void Expire_from_pending_fails()
    {
        var subscription = CreatePending();

        var result = subscription.Expire(Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ChangePlan_from_active_updates_plan_and_price()
    {
        var subscription = CreatePending();
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        var newPlanId = PlanId.From(Guid.NewGuid());
        var newPriceId = PriceId.From(Guid.NewGuid());

        var result = subscription.ChangePlan(newPlanId, newPriceId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPlanId, subscription.PlanId);
        Assert.Equal(newPriceId, subscription.PriceId);
        Assert.Contains(subscription.DomainEvents, e => e.GetType().Name == "PlanChangedDomainEvent");
    }

    [Fact]
    public void ChangePlan_from_non_active_fails()
    {
        var subscription = CreatePending();

        var result = subscription.ChangePlan(PlanId.From(Guid.NewGuid()), PriceId.From(Guid.NewGuid()), Now);

        Assert.True(result.IsFailure);
    }

    /// <summary>Drives a freshly-created (Pending) subscription to <paramref name="status"/> via legal transitions.</summary>
    private static void MoveTo(Subscription subscription, SubscriptionStatus status)
    {
        switch (status)
        {
            case SubscriptionStatus.Pending:
                return;
            case SubscriptionStatus.Trial:
                subscription.StartTrial(Now.AddDays(14), Now);
                return;
            case SubscriptionStatus.Active:
                subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
                return;
            case SubscriptionStatus.PastDue:
                subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
                subscription.MarkPastDue(Now);
                return;
            case SubscriptionStatus.Suspended:
                subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
                subscription.MarkPastDue(Now);
                subscription.Suspend(Now);
                return;
            case SubscriptionStatus.Cancelled:
                subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
                subscription.Cancel(Now);
                return;
            case SubscriptionStatus.Expired:
                subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
                subscription.Cancel(Now);
                subscription.Expire(Now.AddMonths(2));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}
