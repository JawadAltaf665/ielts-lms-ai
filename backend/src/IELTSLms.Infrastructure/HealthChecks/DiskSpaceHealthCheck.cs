using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IELTSLms.Infrastructure.HealthChecks;

/// <summary>
/// Reports available disk space on the application directory's drive.
/// Degrades at &lt;500 MB, unhealthy at &lt;100 MB.
/// </summary>
public sealed class DiskSpaceHealthCheck : IHealthCheck
{
    private const long DegradedThresholdBytes = 500L * 1024 * 1024;
    private const long UnhealthyThresholdBytes = 100L * 1024 * 1024;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");

        var data = new Dictionary<string, object>
        {
            ["drive"] = drive.Name,
            ["availableMB"] = drive.AvailableFreeSpace / (1024 * 1024),
            ["totalMB"] = drive.TotalSize / (1024 * 1024),
        };

        if (drive.AvailableFreeSpace < UnhealthyThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Disk space critically low: {drive.AvailableFreeSpace / (1024 * 1024)} MB remaining.", data: data));
        }

        if (drive.AvailableFreeSpace < DegradedThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Disk space low: {drive.AvailableFreeSpace / (1024 * 1024)} MB remaining.", data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Disk space OK: {drive.AvailableFreeSpace / (1024 * 1024)} MB available.", data));
    }
}
