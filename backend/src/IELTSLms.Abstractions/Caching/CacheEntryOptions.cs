namespace IELTSLms.Abstractions.Caching;

/// <summary>
/// Expiration options for a cache entry. Supply absolute, sliding, or both.
/// When both are set, the entry expires at whichever triggers first.
/// </summary>
public sealed class CacheEntryOptions
{
    /// <summary>Entry expires this long after creation, regardless of access.</summary>
    public TimeSpan? AbsoluteExpiration { get; init; }

    /// <summary>Entry expires if not accessed within this window.</summary>
    public TimeSpan? SlidingExpiration { get; init; }

    /// <summary>Pre-built options: 5-minute absolute expiration (the default).</summary>
    public static readonly CacheEntryOptions Default = new()
    {
        AbsoluteExpiration = TimeSpan.FromMinutes(5),
    };

    /// <summary>Short-lived: 1-minute absolute.</summary>
    public static readonly CacheEntryOptions Brief = new()
    {
        AbsoluteExpiration = TimeSpan.FromMinutes(1),
    };

    /// <summary>Long-lived: 30-minute absolute.</summary>
    public static readonly CacheEntryOptions Extended = new()
    {
        AbsoluteExpiration = TimeSpan.FromMinutes(30),
    };

    /// <summary>Sliding 10-minute window with 1-hour absolute ceiling.</summary>
    public static readonly CacheEntryOptions Sliding = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(10),
        AbsoluteExpiration = TimeSpan.FromHours(1),
    };
}
