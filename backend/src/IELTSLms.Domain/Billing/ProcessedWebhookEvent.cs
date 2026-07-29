using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Idempotency ledger for inbound Stripe webhook events (ERD ENT-039P) — distinct from the
/// domain-event outbox. Not tenant-scoped: a provider event may span tenants.
/// </summary>
public sealed class ProcessedWebhookEvent : Entity<string>
{
    private ProcessedWebhookEvent(string stripeEventId, DateTimeOffset processedAtUtc)
        : base(stripeEventId)
    {
        ProcessedAtUtc = processedAtUtc;
    }

    private ProcessedWebhookEvent()
    {
    }

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static ProcessedWebhookEvent Create(string stripeEventId, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeEventId);
        return new ProcessedWebhookEvent(stripeEventId, nowUtc);
    }
}
