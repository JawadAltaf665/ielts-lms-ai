using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning;

/// <summary>Shared business errors for the Sprint 9 Study Planner / Weakness Detection / Learning
/// Recommendations capabilities — same "one Errors class per feature area" convention as <c>AiErrors</c>.</summary>
internal static class LearningErrors
{
    /// <summary>Below the minimum-data threshold (business rule) — a clear status, not a low-confidence
    /// guess dressed as a real profile.</summary>
    public static readonly Error InsufficientData = Error.Validation(
        "BUSINESS_WEAKNESS_PROFILE_INSUFFICIENT_DATA",
        "Not enough activity yet to compute a weakness profile. Complete a few more exercises across at least two skills first.");

    public static readonly Error WeaknessProfileNotFound = Error.NotFound(
        "NOT_FOUND_WEAKNESS_PROFILE", "No weakness profile found for this learner.");

    public static readonly Error RecommendationNotFound = Error.NotFound(
        "NOT_FOUND_LEARNING_RECOMMENDATION", "No active recommendation set found for this learner.");

    public static readonly Error DailyPlanNotFound = Error.NotFound(
        "NOT_FOUND_DAILY_PLAN", "No plan found for this date.");

    public static readonly Error DailyPlanItemNotFound = Error.NotFound(
        "NOT_FOUND_DAILY_PLAN_ITEM", "Plan item not found.");

    public static readonly Error DailyPlanItemAlreadyCompleted = Error.Conflict(
        "CONFLICT_DAILY_PLAN_ITEM_ALREADY_COMPLETED", "This plan item is already marked complete.");

    public static readonly Error DailyPlanGenerationRaceLost = Error.Conflict(
        "CONFLICT_DAILY_PLAN_ALREADY_EXISTS", "Today's plan already exists.");

    public static readonly Error Forbidden = Error.Forbidden(
        "AUTHZ_FORBIDDEN", "You do not have permission to access this learner's data.");
}
