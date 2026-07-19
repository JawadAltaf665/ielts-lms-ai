using System.Reflection;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence;

/// <summary>
/// The single EF Core context and unit of work for the application. Entity configurations are
/// discovered from this assembly, and domain events raised by aggregates are dispatched inside the
/// same transaction as the save (Backend Architecture §5.5 / §9).
/// </summary>
/// <remarks>
/// No business entities are mapped yet; feature slices add their aggregates and
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes over time.
/// </remarks>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher)
    : DbContext(options), IUnitOfWork
{
    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

        SnakeCaseNamingConvention.Apply(modelBuilder);
    }
}
