using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Analytics.Queries.GetAttemptReview;

/// <summary>Pulls one past attempt + its AI feedback for review (sprint API table). <see cref="Type"/>
/// ∈ writing|speaking|reading|listening|grammar|vocabulary|mock-exam.</summary>
public sealed record GetAttemptReviewQuery(Guid TenantId, Guid UserId, string Type, Guid Id) : IQuery<AttemptReviewResponse>;

/// <summary>One shape covering every attempt type — fields that don't apply to a given
/// <see cref="Type"/> (e.g. <see cref="Confidence"/> for a non-AI-scored Reading attempt) are
/// <see langword="null"/> rather than a per-type polymorphic response.</summary>
public sealed record AttemptReviewResponse(
    string Type, Guid Id, DateTimeOffset AtUtc, string Status, decimal? Band, int? RawScore, int? MaxScore,
    decimal? Confidence, string? FeedbackText, bool? RequiresHumanReview);
