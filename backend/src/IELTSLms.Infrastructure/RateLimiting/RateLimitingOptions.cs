using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.RateLimiting;

/// <summary>
/// Binds the <c>RateLimiting</c> configuration section. All limiter parameters are config-driven
/// so they can be tuned per-environment without redeployment (API Design Standards §8).
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public GlobalLimiterOptions Global { get; set; } = new();
    public PerIpLimiterOptions PerIp { get; set; } = new();
    public PerUserLimiterOptions PerUser { get; set; } = new();
    public PerTenantLimiterOptions PerTenant { get; set; } = new();
}

public sealed class GlobalLimiterOptions
{
    [Range(1, 100000)]
    public int PermitLimit { get; set; } = 1000;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 100;
}

public sealed class PerIpLimiterOptions
{
    [Range(1, 10000)]
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 6;
    public int QueueLimit { get; set; } = 10;
}

public sealed class PerUserLimiterOptions
{
    [Range(1, 10000)]
    public int PermitLimit { get; set; } = 200;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 20;
}

public sealed class PerTenantLimiterOptions
{
    [Range(1, 100000)]
    public int PermitLimit { get; set; } = 500;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 50;
}
