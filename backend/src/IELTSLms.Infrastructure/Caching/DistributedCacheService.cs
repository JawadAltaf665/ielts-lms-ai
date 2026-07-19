using System.Text.Json;
using IELTSLms.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace IELTSLms.Infrastructure.Caching;

/// <summary>
/// <see cref="ICacheService"/> over <see cref="IDistributedCache"/> (Redis in deployed
/// environments, in-memory for local/tests) with JSON serialization. Callers own key namespacing,
/// including tenant scoping, so cached data cannot leak across tenants (Backend Architecture §11).
/// </summary>
internal sealed class DistributedCacheService(IDistributedCache cache) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        return bytes is null ? null : JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
        };

        await cache.SetAsync(key, bytes, options, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        var created = await factory(cancellationToken).ConfigureAwait(false);
        await SetAsync(key, created, expiration, cancellationToken).ConfigureAwait(false);
        return created;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(key, cancellationToken);
}
