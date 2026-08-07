using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.GetAiUsageTrends;

public sealed class GetAiUsageTrendsQueryHandler(IAiResponseRepository repository)
    : IQueryHandler<GetAiUsageTrendsQuery, IReadOnlyList<AiUsageSummaryRow>>
{
    public async Task<Result<IReadOnlyList<AiUsageSummaryRow>>> Handle(
        GetAiUsageTrendsQuery request, CancellationToken cancellationToken)
    {
        var trends = await repository
            .GetUsageTrendsAsync(request.TenantId, request.FromUtc, request.ToUtc, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(trends);
    }
}
