using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.GetBandPrediction;

/// <summary>Gets a band prediction result (Agentic AI Architecture §13.6). Scoped to the caller's own
/// prediction, or (teacher) any prediction in the tenant — mirrors <c>GetWritingSubmissionQuery</c>.</summary>
public sealed record GetBandPredictionQuery(Guid TenantId, Guid UserId, Guid BandPredictionId) : IQuery<BandPredictionResponse>;

public sealed record BandPredictionResponse(
    Guid Id, Guid UserId, string Status, decimal? OverallBand, decimal? ReadingBand, decimal? ListeningBand,
    decimal? WritingBand, decimal? SpeakingBand, decimal? Confidence, bool RecommendMorePractice, DateTimeOffset? CompletedAtUtc,
    string? ErrorCode, string? ErrorMessage);
