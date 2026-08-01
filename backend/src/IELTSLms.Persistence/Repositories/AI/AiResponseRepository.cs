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
