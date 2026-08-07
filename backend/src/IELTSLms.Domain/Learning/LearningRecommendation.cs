using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Learning;

/// <summary>
/// A ranked set of "next action" suggestions for a learner (Sprint 9; AI Architecture §13 "Learning
/// Recommendations"; Agentic AI Architecture §12.3 Learning Recommendation Agent). A newer generation
/// <see cref="Supersede"/>s the previous <see cref="LearningRecommendationStatus.Active"/> row rather
/// than deleting it (ERD note: "keeps a traceable history for later analytics", Sprint 10);
/// <see cref="Dismiss"/> records a learner explicitly rejecting the whole set so the next generation's
/// context can avoid immediately re-suggesting it.
/// </summary>
public sealed class LearningRecommendation : TenantAuditableEntity<LearningRecommendationId>
{
    private LearningRecommendation(
        LearningRecommendationId id, Guid tenantId, Guid userId, string rankedItemsJson, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        RankedItemsJson = rankedItemsJson;
        Status = LearningRecommendationStatus.Active;
        AiResponseId = aiResponseId;
        GeneratedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private LearningRecommendation()
    {
        RankedItemsJson = null!;
    }

    public Guid UserId { get; private init; }

    /// <summary>Structured JSON: a list of <c>{ skill, title, rationale, priority }</c> — see
    /// <c>RecommendationItemDto</c> (Application).</summary>
    public string RankedItemsJson { get; private init; }

    public LearningRecommendationStatus Status { get; private set; }

    public AiResponseId? AiResponseId { get; private init; }

    public DateTimeOffset GeneratedAtUtc { get; private init; }

    public static Result<LearningRecommendation> Create(
        LearningRecommendationId id, Guid tenantId, Guid userId, string rankedItemsJson, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(rankedItemsJson))
        {
            return Result.Failure<LearningRecommendation>(LearningRecommendationDomainErrors.ItemsRequired);
        }

        return Result.Success(new LearningRecommendation(id, tenantId, userId, rankedItemsJson, aiResponseId, nowUtc));
    }

    /// <summary>A newer set has been generated — retire this one for history/audit (ERD note), not deleted.</summary>
    public Result Supersede(DateTimeOffset nowUtc)
    {
        if (Status != LearningRecommendationStatus.Active)
        {
            return Result.Failure(LearningRecommendationDomainErrors.NotActive);
        }

        Status = LearningRecommendationStatus.Superseded;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>The learner explicitly rejected this set — feeds the next generation's "don't
    /// immediately re-recommend this" context (business rule).</summary>
    public Result Dismiss(DateTimeOffset nowUtc)
    {
        if (Status != LearningRecommendationStatus.Active)
        {
            return Result.Failure(LearningRecommendationDomainErrors.NotActive);
        }

        Status = LearningRecommendationStatus.Dismissed;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class LearningRecommendationDomainErrors
{
    public static readonly Error ItemsRequired = Error.Validation(
        "VALIDATION_LEARNING_RECOMMENDATION_ITEMS_REQUIRED", "Ranked recommendation items are required.");

    public static readonly Error NotActive = Error.Conflict(
        "CONFLICT_LEARNING_RECOMMENDATION_NOT_ACTIVE", "Only an active recommendation set can be superseded or dismissed.");
}
