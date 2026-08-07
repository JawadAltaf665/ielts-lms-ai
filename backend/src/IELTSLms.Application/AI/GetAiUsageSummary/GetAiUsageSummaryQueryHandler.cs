using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.GetAiUsageSummary;

public sealed class GetAiUsageSummaryQueryHandler(IAiResponseRepository responses, IClock clock)
    : IQueryHandler<GetAiUsageSummaryQuery, AiUsageSummaryResponse>
{
    private const int DefaultLookbackDays = 30;

    public async Task<Result<AiUsageSummaryResponse>> Handle(GetAiUsageSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var to = request.To ?? today;
        var from = request.From ?? to.AddDays(-DefaultLookbackDays);

        if (from > to)
        {
            return Result.Failure<AiUsageSummaryResponse>(AiErrors.UsageSummaryInvalidRange);
        }

        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtcExclusive = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        // The repository already enforces tenant scope as a hard filter, never a hint (INV cross-tenant
        // protection) — this handler never widens it to a cross-tenant query.
        var rows = await responses
            .GetUsageSummaryAsync(request.TenantId, fromUtc, toUtcExclusive, cancellationToken)
            .ConfigureAwait(false);

        var items = rows
            .Select(r => new AiUsageSummaryItemResponse(
                r.Capability, r.Provider, r.Day, r.RequestCount, r.TotalPromptTokens, r.TotalCompletionTokens,
                r.TotalCostMinorUnits, r.CostCurrency))
            .ToList();

        return Result.Success(new AiUsageSummaryResponse(items));
    }
}
