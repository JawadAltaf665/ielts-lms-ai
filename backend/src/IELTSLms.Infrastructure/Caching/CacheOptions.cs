using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Caching;

/// <summary>
/// Binds the <c>Cache</c> configuration section. Controls default expiration policies,
/// Redis instance prefix, and statistics collection (Backend Architecture §11).
/// </summary>
public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    /// <summary>Redis key prefix to isolate this application's keys from others on the same cluster.</summary>
    [Required]
    public string InstancePrefix { get; set; } = "ielts-lms:";

    /// <summary>Default absolute expiration when callers don't specify.</summary>
    [Range(typeof(TimeSpan), "00:00:10", "24:00:00")]
    public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Enable tracking of cache hit/miss statistics via OpenTelemetry metrics.</summary>
    public bool EnableStatistics { get; set; } = true;
}
