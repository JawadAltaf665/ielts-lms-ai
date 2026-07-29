using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetPlans;

public sealed class GetPlansQueryHandler(IPlanRepository plans, IPriceRepository prices)
    : IQueryHandler<GetPlansQuery, IReadOnlyList<PlanSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<PlanSummaryResponse>>> Handle(
        GetPlansQuery request, CancellationToken cancellationToken)
    {
        var activePlans = await plans.ListActiveAsync(cancellationToken).ConfigureAwait(false);

        var responses = new List<PlanSummaryResponse>(activePlans.Count);
        foreach (var plan in activePlans)
        {
            var planPrices = await prices.ListByPlanAsync(plan.Id, cancellationToken).ConfigureAwait(false);
            responses.Add(new PlanSummaryResponse(
                plan.Id.Value,
                plan.Code.ToString(),
                plan.Name,
                plan.DisplayOrder,
                plan.TrialEligible,
                plan.TrialDays,
                planPrices.Where(p => p.IsActive).Select(ToPriceResponse).ToArray()));
        }

        return Result.Success<IReadOnlyList<PlanSummaryResponse>>(responses);
    }

    private static PriceResponse ToPriceResponse(Price price) => new(
        price.Id.Value, price.PlanId.Value, price.Amount.Currency, price.Amount.AmountMinorUnits, price.Interval.ToString());
}
