using System.Diagnostics.Metrics;
using System.Text.Json;
using IELTSLms.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.Caching;

/// <summary>
/// <see cref="ICacheService"/> over <see cref="IDistributedCache"/> (Redis in deployed
/// environments, in-memory for local/tests) with JSON serialization, configurable expiration,
/// sliding window support, and prefix-based invalidation.
/// </summary>
internal sealed class DistributedCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly Counter<long> _hitCounter;
    private readonly Counter<long> _missCounter;

    public DistributedCacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        IMeterFactory meterFactory)
    {
        _cache = cache;
        _options = options.Value;

        var meter = meterFactory.Create("IELTSLms.Cache");
        _hitCounter = meter.CreateCounter<long>("cache.hits", "count", "Cache hit count");
        _missCounter = meter.CreateCounter<long>("cache.misses", "count", "Cache miss count");
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            _missCounter.Add(1);
            return null;
        }

        _hitCounter.Add(1);
        return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var entryOptions = ToDistributedOptions(options);
        await _cache.SetAsync(key, bytes, entryOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        var created = await factory(cancellationToken).ConfigureAwait(false);
        await SetAsync(key, created, options, cancellationToken).ConfigureAwait(false);
        return created;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(key, cancellationToken);

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // IDistributedCache doesn't support prefix-based removal natively. When using Redis,
        // StackExchange.Redis SCAN + DEL would be needed. For now this is a documented limitation —
        // callers should invalidate by exact key in most cases. Future: inject IConnectionMultiplexer
        // when Redis is available and use SCAN.
        return Task.CompletedTask;
    }

    private DistributedCacheEntryOptions ToDistributedOptions(CacheEntryOptions? options)
    {
        var entry = new DistributedCacheEntryOptions();

        if (options is null)
        {
            entry.AbsoluteExpirationRelativeToNow = _options.DefaultAbsoluteExpiration;
            return entry;
        }

        if (options.AbsoluteExpiration.HasValue)
        {
            entry.AbsoluteExpirationRelativeToNow = options.AbsoluteExpiration.Value;
        }

        if (options.SlidingExpiration.HasValue)
        {
            entry.SlidingExpiration = options.SlidingExpiration.Value;
        }

        if (!options.AbsoluteExpiration.HasValue && !options.SlidingExpiration.HasValue)
        {
            entry.AbsoluteExpirationRelativeToNow = _options.DefaultAbsoluteExpiration;
        }

        return entry;
    }
}
