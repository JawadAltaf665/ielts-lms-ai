using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Suspends subscriptions whose grace period has elapsed while past-due (13 §5, §12: dunning).
/// Stripe's own Smart Retries drive the charge retry cadence; this job only enforces the local access
/// cutoff once the configured grace window has passed without recovery.
/// </summary>
public sealed partial class PastDueSuspensionSweepJob(
    ISubscriptionRepository subscriptions, IUnitOfWork unitOfWork, ITenantContextAccessor tenantContext,
    IClock clock, ILogger<PastDueSuspensionSweepJob> logger)
{
    /// <summary>How long a subscription may remain past-due (full access) before access is suspended.</summary>
    private static readonly TimeSpan GracePeriod = TimeSpan.FromDays(7);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var elapsed = await subscriptions.ListPastDueBeforeAsync(now - GracePeriod, cancellationToken).ConfigureAwait(false);

        var suspendedCount = 0;
        foreach (var subscription in elapsed)
        {
            if (subscription.Suspend(now).IsSuccess)
            {
                suspendedCount++;
            }
        }

        if (suspendedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        SweepCompleted(logger, elapsed.Count, suspendedCount);
    }

    [LoggerMessage(EventId = 3202, Level = LogLevel.Information,
        Message = "Past-due suspension sweep: {Candidates} candidates, {Suspended} suspended.")]
    private static partial void SweepCompleted(ILogger logger, int candidates, int suspended);
}
