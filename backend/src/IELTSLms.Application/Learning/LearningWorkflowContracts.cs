using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Learning;

/// <summary>
/// The structured shapes stored as opaque JSON on <see cref="WeaknessProfile.SkillScoresJson"/>,
/// <see cref="LearningRecommendation.RankedItemsJson"/>, and <see cref="DailyPlan.PlanItemsJson"/>
/// (Sprint 9) — same "domain entity stores raw JSON, Application owns the typed shape" convention as
/// <c>QuestionVariantDraft.DraftContent</c>/<c>QuestionVariantContent</c> (Sprint 8).
/// </summary>
public sealed record SkillWeaknessScoreDto(LearningSkill Skill, decimal Score, int SampleSize);

/// <param name="Priority">1 = highest priority.</param>
public sealed record RecommendationItemDto(LearningSkill Skill, string Title, string Rationale, int Priority);

public sealed record DailyPlanItemDto(
    Guid Id, LearningSkill Skill, string Title, string Description, int DurationMinutes,
    bool IsCompleted, DateTimeOffset? CompletedAtUtc);
