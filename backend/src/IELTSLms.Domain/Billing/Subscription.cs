using IELTSLms.Domain.Billing.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// A tenant's subscription state machine (ERD ENT-040, 13 §4):
/// Pending → Trial/Active → PastDue → Suspended/Cancelled → Expired, with Resume back to Active.
/// </summary>
public sealed class Subscription : TenantAuditableEntity<SubscriptionId>
{
    private Subscription(
        SubscriptionId id, Guid tenantId, BillingCustomerId billingCustomerId, PlanId planId, PriceId priceId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        BillingCustomerId = billingCustomerId;
        PlanId = planId;
        PriceId = priceId;
        Status = SubscriptionStatus.Pending;
        CreatedAtUtc = nowUtc;
    }

    private Subscription()
    {
    }

    public BillingCustomerId BillingCustomerId { get; private set; }

    public PlanId PlanId { get; private set; }

    public PriceId PriceId { get; private set; }

    public string? StripeSubscriptionId { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public DateTimeOffset? CurrentPeriodStart { get; private set; }

    public DateTimeOffset? CurrentPeriodEnd { get; private set; }

    public DateTimeOffset? TrialEndsAtUtc { get; private set; }

    public bool CancelAtPeriodEnd { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public static Subscription Create(
        SubscriptionId id, Guid tenantId, BillingCustomerId billingCustomerId, PlanId planId, PriceId priceId, DateTimeOffset nowUtc)
    {
        var subscription = new Subscription(id, tenantId, billingCustomerId, planId, priceId, nowUtc);
        subscription.RaiseDomainEvent(new SubscriptionCreatedDomainEvent(tenantId, id.Value, planId.Value));
        return subscription;
    }

    /// <summary>Pending → Trial. Raises <see cref="TrialStartedDomainEvent"/>.</summary>
    public Result StartTrial(DateTimeOffset trialEndsAtUtc, DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.Pending)
        {
            return InvalidTransition("Only a pending subscription can start a trial.");
        }

        Status = SubscriptionStatus.Trial;
        TrialEndsAtUtc = trialEndsAtUtc;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new TrialStartedDomainEvent(TenantId, Id.Value, trialEndsAtUtc));
        return Result.Success();
    }

    /// <summary>
    /// Pending/Trial/PastDue → Active. Raises <see cref="TrialEndedDomainEvent"/> (if converting from
    /// trial), <see cref="SubscriptionActivatedDomainEvent"/>, and, when <paramref name="fromCheckout"/>,
    /// <see cref="CheckoutCompletedDomainEvent"/>.
    /// </summary>
    public Result Activate(
        DateTimeOffset currentPeriodStart, DateTimeOffset currentPeriodEnd, string? stripeSubscriptionId,
        DateTimeOffset nowUtc, bool fromCheckout = false)
    {
        if (Status is not (SubscriptionStatus.Pending or SubscriptionStatus.Trial or SubscriptionStatus.PastDue))
        {
            return InvalidTransition("Only a pending, trialing, or past-due subscription can be activated.");
        }

        var wasTrialing = Status is SubscriptionStatus.Trial;

        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        StripeSubscriptionId = stripeSubscriptionId ?? StripeSubscriptionId;
        UpdatedAtUtc = nowUtc;

        if (wasTrialing)
        {
            RaiseDomainEvent(new TrialEndedDomainEvent(TenantId, Id.Value));
        }

        RaiseDomainEvent(new SubscriptionActivatedDomainEvent(TenantId, Id.Value));

        if (fromCheckout)
        {
            RaiseDomainEvent(new CheckoutCompletedDomainEvent(TenantId, Id.Value));
        }

        return Result.Success();
    }

    /// <summary>Active → Active (extends the period). Raises <see cref="SubscriptionRenewedDomainEvent"/>.</summary>
    public Result Renew(DateTimeOffset newPeriodStart, DateTimeOffset newPeriodEnd, DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.Active)
        {
            return InvalidTransition("Only an active subscription can be renewed.");
        }

        CurrentPeriodStart = newPeriodStart;
        CurrentPeriodEnd = newPeriodEnd;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new SubscriptionRenewedDomainEvent(TenantId, Id.Value, newPeriodEnd));
        return Result.Success();
    }

    /// <summary>Active → PastDue, on a failed renewal charge (13 §5 grace period begins).</summary>
    public Result MarkPastDue(DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.Active)
        {
            return InvalidTransition("Only an active subscription can become past-due.");
        }

        Status = SubscriptionStatus.PastDue;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>PastDue → Suspended, once the grace period elapses unpaid.</summary>
    public Result Suspend(DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.PastDue)
        {
            return InvalidTransition("Only a past-due subscription can be suspended.");
        }

        Status = SubscriptionStatus.Suspended;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>
    /// Trial/Active/PastDue → Cancelled. Access continues until the current period ends (13 §4).
    /// Raises <see cref="SubscriptionCancelledDomainEvent"/>.
    /// </summary>
    public Result Cancel(DateTimeOffset nowUtc)
    {
        if (Status is not (SubscriptionStatus.Trial or SubscriptionStatus.Active or SubscriptionStatus.PastDue))
        {
            return InvalidTransition("Only a trialing, active, or past-due subscription can be cancelled.");
        }

        Status = SubscriptionStatus.Cancelled;
        CancelAtPeriodEnd = true;
        CancelledAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new SubscriptionCancelledDomainEvent(TenantId, Id.Value));
        return Result.Success();
    }

    /// <summary>Cancelled → Active, reversing a cancellation before the current period ends.</summary>
    public Result Resume(DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.Cancelled)
        {
            return InvalidTransition("Only a cancelled subscription can be resumed.");
        }

        if (CurrentPeriodEnd is not null && CurrentPeriodEnd <= nowUtc)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_SUBSCRIPTION_PERIOD_ENDED", "Cannot resume a subscription past its current billing period."));
        }

        Status = SubscriptionStatus.Active;
        CancelAtPeriodEnd = false;
        CancelledAtUtc = null;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>
    /// Cancelled/Suspended/PastDue/Trial → Expired. Raises <see cref="SubscriptionExpiredDomainEvent"/>.
    /// </summary>
    public Result Expire(DateTimeOffset nowUtc)
    {
        if (Status is not (SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended or SubscriptionStatus.PastDue or SubscriptionStatus.Trial))
        {
            return InvalidTransition("Only a cancelled, suspended, past-due, or trialing subscription can expire.");
        }

        Status = SubscriptionStatus.Expired;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new SubscriptionExpiredDomainEvent(TenantId, Id.Value));
        return Result.Success();
    }

    /// <summary>Active → Active on a different plan/price (upgrade/downgrade). Raises <see cref="PlanChangedDomainEvent"/>.</summary>
    public Result ChangePlan(PlanId newPlanId, PriceId newPriceId, DateTimeOffset nowUtc)
    {
        if (Status is not SubscriptionStatus.Active)
        {
            return InvalidTransition("Only an active subscription can change plans.");
        }

        var oldPlanId = PlanId;
        PlanId = newPlanId;
        PriceId = newPriceId;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new PlanChangedDomainEvent(TenantId, Id.Value, oldPlanId.Value, newPlanId.Value));
        return Result.Success();
    }

    private static Result InvalidTransition(string message) =>
        Result.Failure(Error.Conflict("CONFLICT_SUBSCRIPTION_STATE", message));
}
