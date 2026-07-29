using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetPlans;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetPricing;

public sealed class GetPricingQueryHandler(IPriceRepository prices) : IQueryHandler<GetPricingQuery, IReadOnlyList<PriceResponse>>
{
    public async Task<Result<IReadOnlyList<PriceResponse>>> Handle(GetPricingQuery request, CancellationToken cancellationToken)
    {
        var activePrices = await prices.ListActiveAsync(cancellationToken).ConfigureAwait(false);

        IReadOnlyList<PriceResponse> responses = activePrices
            .Select(p => new PriceResponse(p.Id.Value, p.PlanId.Value, p.Amount.Currency, p.Amount.AmountMinorUnits, p.Interval.ToString()))
            .ToArray();

        return Result.Success(responses);
    }
}
