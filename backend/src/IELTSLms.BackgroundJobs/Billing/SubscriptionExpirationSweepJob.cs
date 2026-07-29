using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Moves Cancelled/Suspended subscriptions whose current period has ended into the terminal Expired
/// state (13 §4), so entitlements/quotas stop being granted once the paid-for period is truly over.
/// </summary>
public sealed partial class SubscriptionExpirationSweepJob(
    ISubscriptionRepository subscriptions, IUnitOfWork unitOfWork, ITenantContextAccessor tenantContext,
    IClock clock, ILogger<SubscriptionExpirationSweepJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var expirable = await subscriptions.ListExpirableBeforeAsync(now, cancellationToken).ConfigureAwait(false);

        var expiredCount = 0;
        foreach (var subscription in expirable)
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

        SweepCompleted(logger, expirable.Count, expiredCount);
    }

    [LoggerMessage(EventId = 3201, Level = LogLevel.Information,
        Message = "Subscription expiration sweep: {Candidates} candidates, {Expired} expired.")]
    private static partial void SweepCompleted(ILogger logger, int candidates, int expired);
}
