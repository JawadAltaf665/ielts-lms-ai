using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IELTSLms.Infrastructure.HealthChecks;

/// <summary>
/// Reports memory usage for the current process.
/// Degrades at &gt;1 GB working set, unhealthy at &gt;2 GB.
/// </summary>
public sealed class MemoryHealthCheck : IHealthCheck
{
    private const long DegradedThresholdBytes = 1L * 1024 * 1024 * 1024;
    private const long UnhealthyThresholdBytes = 2L * 1024 * 1024 * 1024;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var process = Process.GetCurrentProcess();
        var workingSetMb = process.WorkingSet64 / (1024 * 1024);
        var gcTotalMb = GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024);

        var data = new Dictionary<string, object>
        {
            ["workingSetMB"] = workingSetMb,
            ["gcHeapMB"] = gcTotalMb,
            ["gen0Collections"] = GC.CollectionCount(0),
            ["gen1Collections"] = GC.CollectionCount(1),
            ["gen2Collections"] = GC.CollectionCount(2),
        };

        if (process.WorkingSet64 > UnhealthyThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Memory critically high: {workingSetMb} MB working set.", data: data));
        }

        if (process.WorkingSet64 > DegradedThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Memory usage high: {workingSetMb} MB working set.", data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory OK: {workingSetMb} MB working set.", data));
    }
}
