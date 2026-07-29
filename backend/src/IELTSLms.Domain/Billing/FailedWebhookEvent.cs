using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Dead-letter ledger for Stripe webhook events whose processing threw (ERD ENT-039T, ADR-032) —
/// distinct from <see cref="ProcessedWebhookEvent"/>, which only records successful/no-op processing.
/// Not tenant-scoped: a provider event may span tenants.
/// </summary>
public sealed class FailedWebhookEvent : Entity<string>
{
    /// <summary>Retries beyond this count stop the automatic retry sweep; the event awaits manual resolution.</summary>
    public const int MaxRetryCount = 5;

    private FailedWebhookEvent(string stripeEventId, string eventType, string rawJson, string error, DateTimeOffset nowUtc)
        : base(stripeEventId)
    {
        EventType = eventType;
        RawJson = rawJson;
        LastError = error;
        RetryCount = 0;
        FailedAtUtc = nowUtc;
    }

    private FailedWebhookEvent()
    {
        EventType = null!;
        RawJson = null!;
        LastError = null!;
    }

    public string EventType { get; private set; }

    public string RawJson { get; private set; }

    public string LastError { get; private set; }

    public int RetryCount { get; private set; }

    public DateTimeOffset FailedAtUtc { get; private set; }

    public DateTimeOffset? LastRetryAtUtc { get; private set; }

    public static FailedWebhookEvent Create(string stripeEventId, string eventType, string rawJson, string error, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeEventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        return new FailedWebhookEvent(stripeEventId, eventType, rawJson, error, nowUtc);
    }

    public bool CanRetry => RetryCount < MaxRetryCount;

    public void RecordRetryAttempt(string? error, DateTimeOffset nowUtc)
    {
        RetryCount++;
        LastRetryAtUtc = nowUtc;
        if (error is not null)
        {
            LastError = error;
        }
    }
}
