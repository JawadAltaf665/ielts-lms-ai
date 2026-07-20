using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Time;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IELTSLms.Persistence.Interceptors;

/// <summary>
/// Populates audit fields (CreatedAt/By, UpdatedAt/By, DeletedAt/By, RestoredAt/By)
/// on SaveChanges for all entities implementing the audit interfaces.
/// Domain-set values are respected; this interceptor fills gaps only.
/// </summary>
internal sealed class AuditableEntityInterceptor(
    ICurrentUserService currentUserService,
    IClock clock) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ApplyAuditFields(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ApplyAuditFields(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ApplyAuditFields(DbContext context)
    {
        var now = clock.UtcNow;
        var userId = currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAtUtc == default)
                    {
                        entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = now;
                    }

                    if (entry.Entity is IHasCreatedBy { CreatedBy: null })
                    {
                        entry.Property(nameof(IHasCreatedBy.CreatedBy)).CurrentValue = userId;
                    }

                    break;

                case EntityState.Modified:
                    entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;

                    if (entry.Entity is IHasUpdatedBy)
                    {
                        entry.Property(nameof(IHasUpdatedBy.UpdatedBy)).CurrentValue = userId;
                    }

                    // Detect restore (IsDeleted was true, now false)
                    if (entry.Entity is IRestorable restorable &&
                        entry.OriginalValues.GetValue<bool>(nameof(ISoftDeletable.IsDeleted)) &&
                        !restorable.IsDeleted)
                    {
                        entry.Property(nameof(IRestorable.RestoredAtUtc)).CurrentValue = now;
                        entry.Property(nameof(IRestorable.RestoredBy)).CurrentValue = userId;
                    }

                    break;
            }
        }
    }
}
