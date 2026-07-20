namespace IELTSLms.Abstractions.Audit;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default);
    Task WriteBatchAsync(IReadOnlyCollection<AuditLogEntry> entries, CancellationToken ct = default);
}

public sealed record AuditLogEntry(
    string EntityName,
    string EntityId,
    string Action,
    Guid? UserId,
    Guid? TenantId,
    DateTimeOffset Timestamp,
    string? Changes,
    string? Source,
    string? CorrelationId,
    string? IpAddress);
