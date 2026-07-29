using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Billing;

/// <summary>
/// Closes out usage-tracking periods (13 §3). A new period's consumption always lands in a fresh
/// <c>UsageRecord</c> row (keyed by TenantId/QuotaDefinitionId/PeriodStart, see
/// <see cref="IELTSLms.Domain.Billing.UsageRecord"/>), so "reset" is naturally the absence of carried-over
/// state rather than mutating old rows — the ERD explicitly retains ended-period rows for usage
/// analytics (09 §6.5.1 ENT-039K). This job's job is observability: surfacing how many usage periods
/// just closed, for the reset/renewal cadence the sprint calls out as a distinct job.
/// </summary>
public sealed partial class QuotaResetSweepJob(
    IUsageRecordRepository usageRecords, ITenantContextAccessor tenantContext, IClock clock, ILogger<QuotaResetSweepJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        using var bypass = tenantContext.BypassTenantFilter();

        var justEnded = await usageRecords.ListEndingBeforeAsync(now, cancellationToken).ConfigureAwait(false);

        PeriodsClosed(logger, justEnded.Count);
    }

    [LoggerMessage(EventId = 3204, Level = LogLevel.Information, Message = "Usage periods closed since last run: {Count}.")]
    private static partial void PeriodsClosed(ILogger logger, int count);
}
