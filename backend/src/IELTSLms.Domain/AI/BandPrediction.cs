using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A standalone, history-based cross-skill band prediction (ERD ENT-053; Agentic AI Architecture
/// §13.6). Unlike a per-submission <c>EstimatedBand</c> (Writing/Speaking evaluations, scoped to one
/// response), this aggregates a learner's recent history across all skills into one forward-looking
/// estimate. Created <see cref="BandPredictionStatus.Pending"/> alongside its <see cref="AiOperation"/>
/// (mirrors <c>MockExamResult.CreatePending</c>) so the client has a resource id to poll immediately;
/// <c>BandPredictionJob</c> fills it in via <see cref="Complete"/>/<see cref="Fail"/>. Below
/// <see cref="LowConfidenceThreshold"/>, <see cref="RecommendMorePractice"/> is set instead of
/// presenting the band as authoritative (DDD §13.14/§18 §12: treat low-confidence AI output the same
/// way Writing/Speaking Examiners do — flag, don't present as fact).
/// </summary>
public sealed class BandPrediction : TenantAuditableEntity<BandPredictionId>
{
    /// <summary>Below this confidence, the prediction is labeled unreliable and more practice is recommended instead of a firm number.</summary>
    public const decimal LowConfidenceThreshold = 0.6m;

    private BandPrediction(BandPredictionId id, Guid tenantId, Guid userId, AiOperationId aiOperationId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        AiOperationId = aiOperationId;
        Status = BandPredictionStatus.Pending;
        CreatedAtUtc = nowUtc;
    }

    private BandPrediction()
    {
    }

    public Guid UserId { get; private init; }

    public AiOperationId AiOperationId { get; private init; }

    public BandPredictionStatus Status { get; private set; }

    public decimal? OverallBand { get; private set; }

    public decimal? ReadingBand { get; private set; }

    public decimal? ListeningBand { get; private set; }

    public decimal? WritingBand { get; private set; }

    public decimal? SpeakingBand { get; private set; }

    /// <summary>Predictor-reported confidence, 0-1. Labeled as an estimate (ERD ENT-053 business rule) — never presented as a guarantee.</summary>
    public decimal? Confidence { get; private set; }

    /// <summary>Set when <see cref="Confidence"/> is below <see cref="LowConfidenceThreshold"/> — surfaced to the learner instead of a firm number.</summary>
    public bool RecommendMorePractice { get; private set; }

    public AiResponseId? AIResponseId { get; private init; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static BandPrediction Enqueue(BandPredictionId id, Guid tenantId, Guid userId, AiOperationId aiOperationId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, aiOperationId, nowUtc);

    private static bool IsValidBand(decimal? band) =>
        band is null || (band is >= 0m and <= 9m && (band.Value * 2m) == Math.Truncate(band.Value * 2m));

    /// <summary>Records the predictor's output and transitions Pending→Ready. Idempotent no-op if already terminal.</summary>
    public Result Complete(
        decimal? overallBand, decimal? readingBand, decimal? listeningBand, decimal? writingBand, decimal? speakingBand,
        decimal confidence, AiResponseId aiResponseId, DateTimeOffset nowUtc)
    {
        if (Status != BandPredictionStatus.Pending)
        {
            return Result.Success();
        }

        if (confidence is < 0m or > 1m)
        {
            return Result.Failure(BandPredictionDomainErrors.ConfidenceOutOfRange);
        }

        foreach (var band in new[] { overallBand, readingBand, listeningBand, writingBand, speakingBand })
        {
            if (!IsValidBand(band))
            {
                return Result.Failure(BandPredictionDomainErrors.BandOutOfRange);
            }
        }

        OverallBand = overallBand;
        ReadingBand = readingBand;
        ListeningBand = listeningBand;
        WritingBand = writingBand;
        SpeakingBand = speakingBand;
        Confidence = confidence;
        RecommendMorePractice = confidence < LowConfidenceThreshold;
        Status = BandPredictionStatus.Ready;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public void Fail(string errorCode, string errorMessage, DateTimeOffset nowUtc)
    {
        if (Status != BandPredictionStatus.Pending)
        {
            return;
        }

        Status = BandPredictionStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}

internal static class BandPredictionDomainErrors
{
    public static readonly Error ConfidenceOutOfRange = Error.Validation(
        "VALIDATION_BAND_PREDICTION_CONFIDENCE_OUT_OF_RANGE", "Confidence must be between 0 and 1.");

    public static readonly Error BandOutOfRange = Error.Validation(
        "VALIDATION_BAND_PREDICTION_BAND_OUT_OF_RANGE", "Predicted bands must be between 0 and 9 in 0.5 increments.");
}
