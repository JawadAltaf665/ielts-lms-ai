using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class AiResponseRepository(ApplicationDbContext dbContext)
    : RepositoryBase<AiResponse, AiResponseId>(dbContext), IAiResponseRepository
{
    public async Task<IReadOnlyList<AiUsageSummaryRow>> GetUsageSummaryAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        return await ExecuteUsageQueryAsync(tenantId, fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AiUsageSummaryRow>> GetUsageTrendsAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        // Same aggregation as usage-summary; the "trends" endpoint is a differently-named consumer
        // of the same data shape — no duplication of query logic.
        return await ExecuteUsageQueryAsync(tenantId, fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AiQualityTrendRow>> GetQualityTrendsAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default)
    {
        var query =
            from response in Set
            where response.TenantId == tenantId && response.CreatedAtUtc >= fromUtc && response.CreatedAtUtc < toUtc
            join prompt in DbContext.Set<AiPrompt>() on response.AiPromptId equals prompt.Id
            group response by new { prompt.Feature, Day = response.CreatedAtUtc.Date }
            into bucket
            select new
            {
                bucket.Key.Feature,
                bucket.Key.Day,
                TotalRequests = bucket.Count(),
                FailedRequests = bucket.Count(r => r.Status == AiResponseStatus.Failed || r.Status == AiResponseStatus.Invalid),
                // EscalatedRequests: AiResponse has no RequiresReview flag — the escalation signal
                // lives on Writing/Speaking evaluation entities. The quality-trend endpoint uses
                // Failed+Invalid as the error-rate proxy available on every capability uniformly.
                // Escalation rate is surfaced as 0 here; a future enhancement can join evaluation
                // tables per-capability when the HITL surface is built (reconciliation note).
                EscalatedRequests = 0,
            };

        var rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return rows
            .Select(r =>
            {
                var failureRate = r.TotalRequests > 0 ? (decimal)r.FailedRequests / r.TotalRequests : (decimal?)null;
                return new AiQualityTrendRow(
                    r.Feature,
                    DateOnly.FromDateTime(r.Day),
                    r.TotalRequests,
                    r.FailedRequests,
                    failureRate,
                    r.EscalatedRequests,
                    EscalationRate: 0m);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<AiUsageSummaryRow>> ExecuteUsageQueryAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken)
    {
        var query =
            from response in Set
            where response.TenantId == tenantId && response.CreatedAtUtc >= fromUtc && response.CreatedAtUtc < toUtc
            join prompt in DbContext.Set<AiPrompt>() on response.AiPromptId equals prompt.Id
            join provider in DbContext.Set<AiProvider>() on response.AiProviderId equals provider.Id
            group response by new { prompt.Feature, provider.Name, Day = response.CreatedAtUtc.Date, response.CostCurrency }
            into bucket
            select new
            {
                bucket.Key.Feature,
                bucket.Key.Name,
                bucket.Key.Day,
                bucket.Key.CostCurrency,
                RequestCount = bucket.Count(),
                TotalPromptTokens = bucket.Sum(r => (long)r.PromptTokens),
                TotalCompletionTokens = bucket.Sum(r => (long)r.CompletionTokens),
                TotalCostMinorUnits = bucket.Sum(r => r.CostMinorUnits),
            };

        var rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        // DateOnly.FromDateTime doesn't translate to SQL, so the DateTime -> DateOnly narrowing happens
        // here, after the grouped aggregation has already run server-side.
        return rows
            .Select(r => new AiUsageSummaryRow(
                r.Feature, r.Name, DateOnly.FromDateTime(r.Day), r.RequestCount, r.TotalPromptTokens,
                r.TotalCompletionTokens, r.TotalCostMinorUnits, r.CostCurrency))
            .ToList();
    }
}
