namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Commits all changes tracked within a single business transaction. Implemented by the
/// persistence layer; domain-event dispatch happens inside <see cref="SaveChangesAsync"/>
/// (Backend Architecture §9, Development Standards §15).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists pending changes and dispatches domain events atomically.</summary>
    /// <returns>The number of state entries written to the store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
