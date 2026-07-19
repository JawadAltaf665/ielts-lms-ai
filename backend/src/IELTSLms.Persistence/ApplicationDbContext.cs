using System.Linq.Expressions;
using System.Reflection;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence;

/// <summary>
/// The single EF Core context and unit of work for the application. Entity configurations are
/// discovered from this assembly, and domain events raised by aggregates are dispatched inside the
/// same transaction as the save (Backend Architecture §5.5 / §9).
/// </summary>
/// <remarks>
/// Cross-cutting persistence concerns live here rather than in per-entity configurations: the
/// mandatory tenant global query filter for every <see cref="ITenantScoped"/> entity
/// (Multi-Tenant Architecture §6), the soft-delete filter for every <see cref="ISoftDeletable"/>
/// entity, and stamping <c>TenantId</c> on inserts of tenant-scoped rows so application code never
/// has to set it by hand.
/// </remarks>
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

        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (domainEvents.Length > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);

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
    /// Applies the tenant and soft-delete global query filters to every applicable entity. Combining
    /// both here (EF Core allows only one filter per entity) keeps the isolation invariant in one
    /// place instead of scattered across configurations. The tenant predicate reads the ambient
    /// <see cref="ITenantContext"/> at query time and honours an explicit, audited bypass; when no
    /// tenant is resolved the filter matches nothing, so a missing tenant fails closed rather than
    /// leaking every tenant's rows (Multi-Tenant Architecture §6.2).
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
            Expression? predicate = null;

            if (isTenantScoped)
            {
                // e => this.TenantContextProperty.IsBypassed
                //      || e.TenantId == this.TenantContextProperty.TenantId
                // The filter must read the value off *this* DbContext instance so EF Core re-evaluates
                // it per query instead of baking the first tenant into the cached model. The accessor
                // property is private, so bind it via an explicit PropertyInfo rather than by name
                // (Expression.Property(_, string) only finds public members).
                var tenantProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var contextConstant = Expression.Constant(this);
                var accessor = Expression.Property(contextConstant, TenantContextPropertyInfo);
                var currentTenant = Expression.Property(accessor, nameof(ITenantContext.TenantId));
                var isBypassed = Expression.Property(accessor, nameof(ITenantContext.IsBypassed));

                // Guid == Guid? comparison: lift the left side to nullable.
                var tenantMatches = Expression.Equal(
                    Expression.Convert(tenantProperty, typeof(Guid?)),
                    currentTenant);

                predicate = Expression.OrElse(isBypassed, tenantMatches);
            }

            if (isSoftDeletable)
            {
                // e => !e.IsDeleted
                var notDeleted = Expression.Not(
                    Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)));

                predicate = predicate is null
                    ? notDeleted
                    : Expression.AndAlso(predicate, notDeleted);
            }

            var filter = Expression.Lambda(predicate!, parameter);
            modelBuilder.Entity(clrType).HasQueryFilter(filter);
        }
    }

    /// <summary>
    /// Exposes the ambient tenant context to query-filter expressions built in
    /// <see cref="ApplyGlobalQueryFilters"/>. The filter reads this at query time so each query sees
    /// the tenant resolved for the current request.
    /// </summary>
    private ITenantContext TenantContextProperty => tenantContext;

    /// <summary>Cached <see cref="PropertyInfo"/> for <see cref="TenantContextProperty"/> used to build the filter.</summary>
    private static readonly PropertyInfo TenantContextPropertyInfo =
        typeof(ApplicationDbContext).GetProperty(
            nameof(TenantContextProperty),
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Stamps <c>TenantId</c> on newly-added <see cref="ITenantScoped"/> entities that have not set it
    /// yet, using the ambient tenant. Entities that already carry a tenant (e.g. set explicitly by a
    /// handler, or a seeded system row) are left untouched. Inserting a tenant-scoped row with no
    /// ambient tenant and no explicit tenant is a programming error and fails loudly.
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
