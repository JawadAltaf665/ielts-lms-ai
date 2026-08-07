using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Learning.Recommendations.Queries.GetRecommendations;

public sealed record GetRecommendationsQuery(Guid TenantId, Guid UserId) : IQuery<RecommendationsResponse>;

/// <param name="Status">"Ready" (an active recommendation set exists), "InsufficientData", or
/// "NotComputedYet" — same status vocabulary as <c>WeaknessProfileResponse</c>.</param>
public sealed record RecommendationsResponse(
    Guid? LearningRecommendationId, string Status, IReadOnlyList<RecommendationItemDto>? Items, DateTimeOffset? GeneratedAtUtc);
