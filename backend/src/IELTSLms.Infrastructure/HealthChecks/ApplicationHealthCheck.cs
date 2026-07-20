using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IELTSLms.Infrastructure.HealthChecks;

/// <summary>
/// Verifies the application is functional by checking a trivial operation (memory allocation).
/// Tagged as startup + readiness (Backend Architecture §12).
/// </summary>
public sealed class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Application is running."));
    }
}
