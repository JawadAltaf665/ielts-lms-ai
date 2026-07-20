namespace IELTSLms.Abstractions.Caching;

/// <summary>
/// Application-facing cache port. The implementation handles serialization, key namespacing,
/// and tenant/user scoping. Callers use <see cref="CacheKeys"/> helpers to build properly
/// namespaced keys (Backend Architecture §11).
/// </summary>
public interface ICacheService
{
    /// <summary>Gets a cached value, or <see langword="null"/> if the key is absent or expired.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Stores a value with configurable expiration.</summary>
    Task SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Fetches from cache or produces and stores the value on a miss.</summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Removes a cached value if present.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Removes all keys matching a prefix (namespace invalidation).</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
