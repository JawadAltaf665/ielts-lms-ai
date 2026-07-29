using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Billing;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Reconciles local subscription state against Stripe's for subscriptions still linked to a provider
/// id, catching drift from a missed or failed webhook delivery (13 §11, §12).
/// </summary>
public sealed partial class StripeReconciliationJob(
    ISubscriptionRepository subscriptions, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext, IClock clock, ILogger<StripeReconciliationJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var candidates = await subscriptions.ListLinkedToProviderAsync(cancellationToken).ConfigureAwait(false);

        var reconciledCount = 0;
        foreach (var subscription in candidates)
        {
            var snapshot = await paymentGateway
                .GetSubscriptionAsync(subscription.StripeSubscriptionId!, cancellationToken).ConfigureAwait(false);
            if (snapshot is null)
            {
                continue;
            }

            if (Reconcile(subscription, snapshot, now))
            {
                reconciledCount++;
            }
        }

        if (reconciledCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        ReconciliationCompleted(logger, candidates.Count, reconciledCount);
    }

    private static bool Reconcile(Subscription subscription, ProviderSubscriptionSnapshot snapshot, DateTimeOffset now)
    {
        return snapshot.Status switch
        {
            "active" when subscription.Status is SubscriptionStatus.Trial or SubscriptionStatus.PastDue =>
                subscription.Activate(snapshot.CurrentPeriodStart, snapshot.CurrentPeriodEnd, subscription.StripeSubscriptionId, now).IsSuccess,
            "active" when subscription.Status is SubscriptionStatus.Active && subscription.CurrentPeriodEnd != snapshot.CurrentPeriodEnd =>
                subscription.Renew(snapshot.CurrentPeriodStart, snapshot.CurrentPeriodEnd, now).IsSuccess,
            "past_due" or "unpaid" when subscription.Status is SubscriptionStatus.Active =>
                subscription.MarkPastDue(now).IsSuccess,
            "canceled" when subscription.Status is not (SubscriptionStatus.Cancelled or SubscriptionStatus.Expired) =>
                subscription.Expire(now).IsSuccess,
            _ => false,
        };
    }

    [LoggerMessage(EventId = 3205, Level = LogLevel.Information,
        Message = "Stripe reconciliation run: {Candidates} candidates checked, {Reconciled} reconciled.")]
    private static partial void ReconciliationCompleted(ILogger logger, int candidates, int reconciled);
}
