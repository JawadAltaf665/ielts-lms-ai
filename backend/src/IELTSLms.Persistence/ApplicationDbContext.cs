using System.Linq.Expressions;
using System.Reflection;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Application.Exceptions;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence;

/// <summary>
/// The single EF Core context and unit of work for the application. Entity configurations are
/// discovered from this assembly, and domain events raised by aggregates are dispatched inside the
/// same transaction as the save (Backend Architecture §5.5 / §9).
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher,
    ITenantContext tenantContext)
    : DbContext(options), IUnitOfWork
{
    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenantOnNewEntities();

        // Capture events before saving so handlers run against the persisted state within the txn.
        var aggregates = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToArray();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToArray();

        // Dispatch pre-commit events before persisting
        var preCommitEvents = domainEvents.Where(e => e is IPreCommitDomainEvent).ToArray();
        if (preCommitEvents.Length > 0)
        {
            await domainEventDispatcher.DispatchAsync(preCommitEvents, cancellationToken).ConfigureAwait(false);
        }

        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Count > 0 ? ex.Entries[0] : null;
            var entityName = entry?.Metadata.ClrType.Name ?? "Unknown";
            var entityId = entry?.Property("Id")?.CurrentValue;
            throw new ConcurrencyConflictException(entityName, entityId);
        }

        // Dispatch post-commit events (default behavior for unmarked events)
        var postCommitEvents = domainEvents.Where(e => e is not IPreCommitDomainEvent).ToArray();
        if (postCommitEvents.Length > 0)
        {
            await domainEventDispatcher.DispatchAsync(postCommitEvents, cancellationToken).ConfigureAwait(false);

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }
        }
        else if (aggregates.Length > 0)
        {
            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        ApplyGlobalQueryFilters(modelBuilder);

        SnakeCaseNamingConvention.Apply(modelBuilder);
    }

    /// <summary>
    /// Applies the tenant and soft-delete global query filters to every applicable entity.
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenantScoped = typeof(ITenantScoped).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (!isTenantScoped && !isSoftDeletable)
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filter = null;

            if (isTenantScoped)
            {
                // e.TenantId == TenantContextProperty.TenantId || TenantContextProperty.BypassTenantFilter
                var tenantIdProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var contextProperty = Expression.Property(
                    Expression.Constant(this),
                    TenantContextPropertyInfo);
                var contextTenantId = Expression.Property(contextProperty, nameof(ITenantContext.TenantId));

                // Coalesce nullable tenant id for comparison
                var coalesce = Expression.Coalesce(contextTenantId, Expression.Constant(Guid.Empty));
                var tenantMatch = Expression.Equal(tenantIdProperty, coalesce);

                // Bypass
                var bypass = Expression.Property(contextProperty, nameof(ITenantContext.IsBypassed));
                filter = Expression.OrElse(bypass, tenantMatch);
            }

            if (isSoftDeletable)
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                filter = filter is not null
                    ? Expression.AndAlso(filter, notDeleted)
                    : notDeleted;
            }

            if (filter is not null)
            {
                var lambda = Expression.Lambda(filter, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Exposes the tenant context as a CLR property so the EF Core filter expression can bind to it.
    /// </summary>
    private ITenantContext TenantContextProperty => tenantContext;

    /// <summary>Cached <see cref="PropertyInfo"/> for <see cref="TenantContextProperty"/> used to build the filter.</summary>
    private static readonly PropertyInfo TenantContextPropertyInfo =
        typeof(ApplicationDbContext).GetProperty(
            nameof(TenantContextProperty),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Stamps <c>TenantId</c> on newly-added <see cref="ITenantScoped"/> entities that have not set it
    /// yet, using the ambient tenant.
    /// </summary>
    private void StampTenantOnNewEntities()
    {
        var added = ChangeTracker.Entries<ITenantScoped>()
            .Where(entry => entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            .ToArray();

        if (added.Length == 0)
        {
            return;
        }

        if (tenantContext.TenantId is not { } tenantId)
        {
            throw new InvalidOperationException(
                "Cannot persist a tenant-scoped entity: no tenant is resolved for the current scope " +
                "and the entity did not set a tenant explicitly.");
        }

        foreach (var entry in added)
        {
            entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue = tenantId;
        }
    }
}
