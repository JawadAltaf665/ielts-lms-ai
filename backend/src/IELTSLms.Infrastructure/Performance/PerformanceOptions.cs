using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Performance;

/// <summary>
/// Binds the <c>Performance</c> configuration section. Controls response compression,
/// request size limits, connection limits, and Kestrel tuning
/// (Infrastructure Deployment §4, Development Standards §19).
/// </summary>
public sealed class PerformanceOptions
{
    public const string SectionName = "Performance";

    /// <summary>Maximum request body size in bytes (default 10 MB).</summary>
    [Range(1024, 104857600)]
    public long MaxRequestBodySizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Request timeout in seconds for Kestrel.</summary>
    [Range(5, 600)]
    public int RequestTimeoutSeconds { get; set; } = 120;

    /// <summary>Maximum concurrent connections per Kestrel endpoint.</summary>
    [Range(10, 100000)]
    public int MaxConcurrentConnections { get; set; } = 5000;

    /// <summary>Maximum concurrent upgraded connections (WebSocket).</summary>
    [Range(10, 100000)]
    public int MaxConcurrentUpgradedConnections { get; set; } = 1000;

    /// <summary>Enable response compression (gzip, brotli).</summary>
    public bool EnableResponseCompression { get; set; } = true;

    /// <summary>Kestrel keep-alive timeout in seconds.</summary>
    [Range(5, 600)]
    public int KeepAliveTimeoutSeconds { get; set; } = 130;

    /// <summary>Kestrel max request header count.</summary>
    [Range(10, 500)]
    public int MaxRequestHeaderCount { get; set; } = 100;

    /// <summary>Kestrel max request header total size in bytes.</summary>
    [Range(1024, 1048576)]
    public int MaxRequestHeadersTotalSizeBytes { get; set; } = 32768;
}
