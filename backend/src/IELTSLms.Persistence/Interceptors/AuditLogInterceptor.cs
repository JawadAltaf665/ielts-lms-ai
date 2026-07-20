using System.Text.Json;
using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Time;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace IELTSLms.Persistence.Interceptors;

/// <summary>
/// Captures entity changes (old/new values) and writes them via IAuditLogWriter.
/// Runs after other interceptors so soft-delete conversion is already applied.
/// </summary>
internal sealed class AuditLogInterceptor(
    IAuditLogWriter auditLogWriter,
    ICurrentUserService currentUserService,
    ICorrelationIdProvider correlationIdProvider,
    IRequestContext requestContext,
    IClock clock,
    IOptions<AuditOptions> auditOptions) : SaveChangesInterceptor
{
    private List<AuditLogEntry>? _pendingEntries;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null && auditOptions.Value.EnableAuditLog)
        {
            _pendingEntries = CaptureChanges(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEntries is { Count: > 0 })
        {
            await auditLogWriter.WriteBatchAsync(_pendingEntries, cancellationToken).ConfigureAwait(false);
            _pendingEntries = null;
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private List<AuditLogEntry> CaptureChanges(DbContext context)
    {
        var entries = new List<AuditLogEntry>();
        var opts = auditOptions.Value;
        var now = clock.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            var entityName = entry.Metadata.ClrType.Name;
            if (opts.ExcludedEntities.Contains(entityName))
            {
                continue;
            }

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified when entry.Entity is ISoftDeletable { IsDeleted: true }
                    && entry.OriginalValues.GetValue<bool>(nameof(ISoftDeletable.IsDeleted)) == false
                    => "Deleted",
                EntityState.Modified when entry.Entity is IRestorable
                    && entry.OriginalValues.GetValue<bool>(nameof(ISoftDeletable.IsDeleted))
                    && entry.Entity is ISoftDeletable { IsDeleted: false }
                    => "Restored",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => null,
            };

            if (action is null)
            {
                continue;
            }

            var entityId = GetPrimaryKeyValue(entry);
            var changes = CapturePropertyChanges(entry, opts.ExcludedProperties);

            entries.Add(new AuditLogEntry(
                entityName,
                entityId,
                action,
                currentUserService.UserId,
                entry.Entity is ITenantScoped ts ? ts.TenantId : null,
                now,
                changes,
                "EfInterceptor",
                correlationIdProvider.CorrelationId,
                requestContext.ClientIp));
        }

        return entries;
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyValues = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
            .ToArray();

        return keyValues is { Length: > 0 } ? string.Join(",", keyValues) : "";
    }

    private static string? CapturePropertyChanges(EntityEntry entry, string[] excludedProperties)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            if (excludedProperties.Contains(property.Metadata.Name))
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    if (property.CurrentValue is not null)
                    {
                        changes[property.Metadata.Name] = new { @new = property.CurrentValue?.ToString() };
                    }
                    break;

                case EntityState.Modified when property.IsModified:
                    changes[property.Metadata.Name] = new
                    {
                        old = property.OriginalValue?.ToString(),
                        @new = property.CurrentValue?.ToString(),
                    };
                    break;

                case EntityState.Deleted:
                    changes[property.Metadata.Name] = new { old = property.OriginalValue?.ToString() };
                    break;
            }
        }

        return changes.Count > 0 ? JsonSerializer.Serialize(changes) : null;
    }
}
