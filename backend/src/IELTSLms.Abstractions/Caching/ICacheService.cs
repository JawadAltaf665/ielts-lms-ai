namespace IELTSLms.Abstractions.Caching;

/// <summary>
/// Application-facing cache port. Keys must be tenant-scoped by the caller so cached data never
/// leaks across tenants (Backend Architecture §11). Implemented by the infrastructure layer.
/// </summary>
public interface ICacheService
{
    /// <summary>Gets a cached value, or <see langword="null"/> if the key is absent or expired.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Stores a value with an optional absolute expiration.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Fetches from cache or produces and stores the value on a miss.</summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Removes a cached value if present.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
