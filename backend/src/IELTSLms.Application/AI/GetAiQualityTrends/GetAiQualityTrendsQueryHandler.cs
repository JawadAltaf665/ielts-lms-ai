using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.GetAiQualityTrends;

public sealed class GetAiQualityTrendsQueryHandler(IAiResponseRepository repository)
    : IQueryHandler<GetAiQualityTrendsQuery, IReadOnlyList<AiQualityTrendRow>>
{
    public async Task<Result<IReadOnlyList<AiQualityTrendRow>>> Handle(
        GetAiQualityTrendsQuery request, CancellationToken cancellationToken)
    {
        var trends = await repository
            .GetQualityTrendsAsync(request.TenantId, request.FromUtc, request.ToUtc, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(trends);
    }
}
