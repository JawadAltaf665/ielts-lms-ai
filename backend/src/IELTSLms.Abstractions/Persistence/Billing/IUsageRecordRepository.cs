using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="UsageRecord"/> — metered per-period counters (ERD ENT-039K).</summary>
public interface IUsageRecordRepository : IRepository<UsageRecord, UsageRecordId>
{
    Task<UsageRecord?> GetAsync(
        Guid tenantId, QuotaDefinitionId quotaDefinitionId, DateTimeOffset periodStart, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsageRecord>> ListByTenantAsync(
        Guid tenantId, DateTimeOffset periodStart, CancellationToken cancellationToken = default);

    /// <summary>All usage records whose period has ended before <paramref name="before"/>, for the quota-reset job.</summary>
    Task<IReadOnlyList<UsageRecord>> ListEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default);

    /// <summary>All usage records for a period, across every tenant, for billing analytics (ADR-032). Requires an active tenant-filter bypass.</summary>
    Task<IReadOnlyList<UsageRecord>> ListByPeriodAsync(DateTimeOffset periodStart, CancellationToken cancellationToken = default);
}
