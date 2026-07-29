using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Reconciliation safety net for trial expiry (13 §4, §12): Stripe normally drives trial→active/
/// past-due transitions via webhooks, but if a webhook is ever missed, a trial left unconverted well
/// past its <c>TrialEndsAtUtc</c> is expired here instead of silently granting indefinite trial access.
/// </summary>
public sealed partial class TrialExpirationSweepJob(
    ISubscriptionRepository subscriptions, IUnitOfWork unitOfWork, ITenantContextAccessor tenantContext,
    IClock clock, ILogger<TrialExpirationSweepJob> logger)
{
    /// <summary>Grace window after <c>TrialEndsAtUtc</c> before a still-trialing subscription is force-expired.</summary>
    private static readonly TimeSpan WebhookGracePeriod = TimeSpan.FromDays(1);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var lapsed = await subscriptions.ListTrialsEndingBeforeAsync(now - WebhookGracePeriod, cancellationToken).ConfigureAwait(false);

        var expiredCount = 0;
        foreach (var subscription in lapsed)
        {
            if (subscription.Expire(now).IsSuccess)
            {
                expiredCount++;
            }
        }

        if (expiredCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        SweepCompleted(logger, lapsed.Count, expiredCount);
    }

    [LoggerMessage(EventId = 3200, Level = LogLevel.Information,
        Message = "Trial expiration sweep: {Candidates} candidates, {Expired} expired.")]
    private static partial void SweepCompleted(ILogger logger, int candidates, int expired);
}
