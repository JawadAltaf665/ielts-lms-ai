using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Sends a heads-up before a trial converts to paid, so the tenant isn't surprised by the first
/// charge (13 §12). Renewal-charge reminders for paid periods are less critical since Stripe emails
/// its own receipts/upcoming-invoice notices; this focuses on the trial-conversion moment, which is
/// entirely our own UX to own.
/// </summary>
public sealed partial class RenewalReminderJob(
    ISubscriptionRepository subscriptions, IBillingCustomerRepository billingCustomers, IEmailSender emailSender,
    ITenantContextAccessor tenantContext, IClock clock, ILogger<RenewalReminderJob> logger)
{
    /// <summary>How far ahead of trial end to send the reminder.</summary>
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromDays(3);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var endingSoon = await subscriptions.ListTrialsEndingBeforeAsync(now + ReminderWindow, cancellationToken).ConfigureAwait(false);

        var sent = 0;
        foreach (var subscription in endingSoon)
        {
            var customer = await billingCustomers.GetByIdAsync(subscription.BillingCustomerId, cancellationToken).ConfigureAwait(false);
            if (customer is null)
            {
                continue;
            }

            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    "Your trial is ending soon",
                    $"<p>Your trial ends on {subscription.TrialEndsAtUtc:d}. It will convert to a paid subscription automatically.</p>",
                    $"Your trial ends on {subscription.TrialEndsAtUtc:d}. It will convert to a paid subscription automatically."),
                cancellationToken).ConfigureAwait(false);
            sent++;
        }

        ReminderRunCompleted(logger, endingSoon.Count, sent);
    }

    [LoggerMessage(EventId = 3203, Level = LogLevel.Information,
        Message = "Trial-ending reminder run: {Candidates} candidates, {Sent} sent.")]
    private static partial void ReminderRunCompleted(ILogger logger, int candidates, int sent);
}
