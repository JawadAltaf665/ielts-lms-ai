using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace IELTSLms.Infrastructure.Observability;

/// <summary>
/// Central registry for custom ActivitySources and Meters used by the application.
/// Each bounded context or infrastructure concern gets its own source so traces can be
/// filtered by subsystem (Observability §2).
/// </summary>
public static class Telemetry
{
    public const string ServiceName = "IELTSLms";

    // Activity sources (tracing)
    public static readonly ActivitySource Application = new($"{ServiceName}.Application");
    public static readonly ActivitySource Infrastructure = new($"{ServiceName}.Infrastructure");
    public static readonly ActivitySource Persistence = new($"{ServiceName}.Persistence");
    public static readonly ActivitySource BackgroundJobs = new($"{ServiceName}.BackgroundJobs");

    // Source names for OTel registration
    public static readonly string[] ActivitySourceNames =
    [
        Application.Name,
        Infrastructure.Name,
        Persistence.Name,
        BackgroundJobs.Name,
    ];

    // Meters (metrics)
    public const string ApplicationMeterName = $"{ServiceName}.Application";
    public const string InfrastructureMeterName = $"{ServiceName}.Infrastructure";
    public const string CacheMeterName = $"{ServiceName}.Cache";
    public const string JobsMeterName = $"{ServiceName}.Jobs";

    public static readonly string[] MeterNames =
    [
        ApplicationMeterName,
        InfrastructureMeterName,
        CacheMeterName,
        JobsMeterName,
    ];
}
