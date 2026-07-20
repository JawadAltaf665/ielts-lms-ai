using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Observability;

/// <summary>
/// Binds the <c>Observability</c> configuration section. Controls the service identity reported to
/// the collector and the OTLP endpoint traces/metrics are exported to (Observability §2 / §4).
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Logical service name reported on all telemetry.</summary>
    [Required]
    public string ServiceName { get; set; } = "ielts-lms-api";

    /// <summary>Service version reported on all telemetry.</summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>Deployment environment (development, staging, production).</summary>
    public string? Environment { get; set; }

    /// <summary>OTLP collector endpoint. When empty, exporting is disabled (e.g. local dev).</summary>
    public string? OtlpEndpoint { get; set; }
}
