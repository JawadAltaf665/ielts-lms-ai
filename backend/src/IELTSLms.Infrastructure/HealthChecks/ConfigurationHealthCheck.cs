using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IELTSLms.Infrastructure.HealthChecks;

/// <summary>
/// Validates that critical configuration sections are present and non-empty at health-check time.
/// Reports which sections are missing so operators can identify misconfiguration quickly.
/// </summary>
public sealed class ConfigurationHealthCheck(IConfiguration configuration) : IHealthCheck
{
    private static readonly string[] RequiredSections =
    [
        "ConnectionStrings:Database",
        "Jwt:Issuer",
        "Jwt:Audience",
        "Serilog:MinimumLevel",
        "Observability:ServiceName",
    ];

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = new List<string>();

        foreach (var section in RequiredSections)
        {
            var value = configuration[section];
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(section);
            }
        }

        if (missing.Count > 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Missing configuration: {string.Join(", ", missing)}.",
                data: new Dictionary<string, object> { ["missingSections"] = missing }));
        }

        return Task.FromResult(HealthCheckResult.Healthy("All required configuration present."));
    }
}
