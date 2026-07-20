using IELTSLms.Abstractions.Audit;
using IELTSLms.Domain.Audit;

namespace IELTSLms.Persistence.Audit;

internal sealed class EfAuditLogWriter(ApplicationDbContext dbContext) : IAuditLogWriter
{
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        var auditLog = MapToEntity(entry);
        dbContext.Set<AuditLog>().Add(auditLog);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task WriteBatchAsync(IReadOnlyCollection<AuditLogEntry> entries, CancellationToken ct = default)
    {
        if (entries.Count == 0)
        {
            return;
        }

        var entities = entries.Select(MapToEntity).ToArray();
        dbContext.Set<AuditLog>().AddRange(entities);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static AuditLog MapToEntity(AuditLogEntry entry) =>
        AuditLog.Create(
            entry.EntityName,
            entry.EntityId,
            Enum.TryParse<AuditAction>(entry.Action, out var action) ? action : AuditAction.Updated,
            entry.UserId,
            entry.TenantId,
            entry.Timestamp,
            entry.Changes,
            entry.Source,
            entry.CorrelationId,
            entry.IpAddress);
}
