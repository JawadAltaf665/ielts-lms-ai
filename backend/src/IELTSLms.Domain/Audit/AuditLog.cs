using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Audit;

/// <summary>
/// Immutable append-only record of an entity change. Captures old/new values as JSON.
/// </summary>
public sealed class AuditLog : Entity<AuditLogId>
{
    private AuditLog() { }

    public static AuditLog Create(
        string entityName,
        string entityId,
        AuditAction action,
        Guid? userId,
        Guid? tenantId,
        DateTimeOffset timestamp,
        string? changes,
        string? source,
        string? correlationId,
        string? ipAddress)
    {
        return new AuditLog
        {
            Id = AuditLogId.New(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            TenantId = tenantId,
            Timestamp = timestamp,
            Changes = changes,
            Source = source,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
        };
    }

    public string EntityName { get; private init; } = default!;
    public string EntityId { get; private init; } = default!;
    public AuditAction Action { get; private init; }
    public Guid? UserId { get; private init; }
    public Guid? TenantId { get; private init; }
    public DateTimeOffset Timestamp { get; private init; }
    public string? Changes { get; private init; }
    public string? Source { get; private init; }
    public string? CorrelationId { get; private init; }
    public string? IpAddress { get; private init; }
}
