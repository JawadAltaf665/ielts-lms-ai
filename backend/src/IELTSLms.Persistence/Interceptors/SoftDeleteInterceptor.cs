using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Time;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IELTSLms.Persistence.Interceptors;

/// <summary>
/// Converts EF Core physical deletes to soft deletes for ISoftDeletable entities.
/// Any call to DbContext.Remove() on a soft-deletable entity sets the flag instead.
/// </summary>
internal sealed class SoftDeleteInterceptor(
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
            ConvertToSoftDelete(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ConvertToSoftDelete(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ConvertToSoftDelete(DbContext context)
    {
        var now = clock.UtcNow;
        var userId = currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>()
                     .Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Property(nameof(ISoftDeletable.IsDeleted)).CurrentValue = true;
            entry.Property(nameof(ISoftDeletable.DeletedAtUtc)).CurrentValue = now;

            if (entry.Entity is IHasDeletedBy)
            {
                entry.Property(nameof(IHasDeletedBy.DeletedBy)).CurrentValue = userId;
            }
        }
    }
}
