using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IELTSLms.Infrastructure.Observability;

/// <summary>
/// Wires OpenTelemetry tracing and metrics for the service. Instrumentation covers ASP.NET Core,
/// outbound HTTP, and the runtime; traces/metrics export via OTLP when an endpoint is configured
/// (Observability §2 / §4).
/// </summary>
internal static class ObservabilityRegistration
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ObservabilityOptions>()
            .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(options.ServiceName, serviceVersion: options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(exporter => exporter.Endpoint = new Uri(options.OtlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(exporter => exporter.Endpoint = new Uri(options.OtlpEndpoint));
                }
            });

        return services;
    }
}
