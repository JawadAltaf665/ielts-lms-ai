using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Billing.ProcessStripeWebhook;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Re-attempts webhook events dead-lettered by <c>ProcessStripeWebhookCommandHandler</c> (ADR-032,
/// 13 §11/§12: retry/dead-letter). Not tenant-scoped work — webhook events aren't tenant-bound, so no
/// <c>BypassTenantFilter</c> is needed here.
/// </summary>
public sealed partial class DeadLetterWebhookRetryJob(
    IFailedWebhookEventRepository failedEvents, ISender sender, ILogger<DeadLetterWebhookRetryJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var retryable = await failedEvents.ListRetryableAsync(cancellationToken).ConfigureAwait(false);

        var succeeded = 0;
        foreach (var failedEvent in retryable)
        {
            var eventId = failedEvent.Id;
            var command = new ProcessStripeWebhookCommand(eventId, failedEvent.EventType, failedEvent.RawJson);

            // The handler always returns Success (it dead-letters internally rather than throwing),
            // so the only reliable signal of a resolved retry is whether the dead-letter row is gone.
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            if (await failedEvents.GetByIdAsync(eventId, cancellationToken).ConfigureAwait(false) is null)
            {
                succeeded++;
            }
        }

        RetrySweepCompleted(logger, retryable.Count, succeeded);
    }

    [LoggerMessage(EventId = 3300, Level = LogLevel.Information,
        Message = "Dead-letter webhook retry sweep: {Candidates} candidates, {Succeeded} handled.")]
    private static partial void RetrySweepCompleted(ILogger logger, int candidates, int succeeded);
}
