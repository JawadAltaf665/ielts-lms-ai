namespace IELTSLms.Persistence.Outbox;

/// <summary>
/// Persisted representation of a domain event for reliable async delivery (outbox pattern).
/// The processor that polls and publishes is a Sprint 5+ concern.
/// </summary>
internal sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public Guid? TenantId { get; init; }
    public string? CorrelationId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
