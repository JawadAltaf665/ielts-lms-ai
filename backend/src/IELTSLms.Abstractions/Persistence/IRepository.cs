using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Collection-like access to an aggregate. One repository per aggregate root; queries that
/// span aggregates or shape read models belong in query handlers, not here
/// (Backend Architecture §5.2 / §5.5).
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>Loads an aggregate by id, or <see langword="null"/> if none exists in the current tenant scope.</summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new aggregate to be persisted on the next unit-of-work commit.</summary>
    void Add(TAggregate aggregate);

    /// <summary>Marks an aggregate for removal on the next unit-of-work commit.</summary>
    void Remove(TAggregate aggregate);
}
