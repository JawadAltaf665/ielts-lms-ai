using IELTSLms.Abstractions.Persistence;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>
/// Shared EF Core implementation of <see cref="IRepository{TAggregate, TId}"/>. Persistence is
/// deferred to the unit of work (<see cref="ApplicationDbContext.SaveChangesAsync"/>); repositories
/// only track intent (Backend Architecture §5.5).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
internal abstract class RepositoryBase<TAggregate, TId>(ApplicationDbContext dbContext)
    : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected ApplicationDbContext DbContext { get; } = dbContext;

    protected DbSet<TAggregate> Set => DbContext.Set<TAggregate>();

    /// <inheritdoc />
    public virtual async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync([id], cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public void Add(TAggregate aggregate) => Set.Add(aggregate);

    /// <inheritdoc />
    public void Remove(TAggregate aggregate) => Set.Remove(aggregate);
}
