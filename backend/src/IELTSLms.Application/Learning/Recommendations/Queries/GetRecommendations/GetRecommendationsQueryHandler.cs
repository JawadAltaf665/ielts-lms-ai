using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Learning.WeaknessDetection;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.Recommendations.Queries.GetRecommendations;

public sealed class GetRecommendationsQueryHandler(
    ILearningRecommendationRepository recommendations, IProgressTrackingRepository progressTracking)
    : IQueryHandler<GetRecommendationsQuery, RecommendationsResponse>
{
    public async Task<Result<RecommendationsResponse>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var active = await recommendations.GetActiveByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        if (active is not null)
        {
            var items = JsonSerializer.Deserialize<List<RecommendationItemDto>>(active.RankedItemsJson, LearningJsonOptions.Instance) ?? [];
            return Result.Success(new RecommendationsResponse(active.Id.Value, "Ready", items, active.GeneratedAtUtc));
        }

        var rows = await progressTracking.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        var status = ProgressAnalysisService.IsSufficient(rows) ? "NotComputedYet" : "InsufficientData";
        return Result.Success(new RecommendationsResponse(null, status, null, null));
    }
}
