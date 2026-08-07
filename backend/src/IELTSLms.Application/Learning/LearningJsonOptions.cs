using System.Text.Json;
using System.Text.Json.Serialization;

namespace IELTSLms.Application.Learning;

/// <summary>
/// Shared <see cref="JsonSerializerOptions"/> for the structured JSON blobs stored on
/// <see cref="Domain.Learning.WeaknessProfile.SkillScoresJson"/>,
/// <see cref="Domain.Learning.LearningRecommendation.RankedItemsJson"/>, and
/// <see cref="Domain.Learning.DailyPlan.PlanItemsJson"/> — <see cref="JsonSerializerOptions.Web"/> plus a
/// string enum converter, so <see cref="Domain.Learning.LearningSkill"/> round-trips as its readable name
/// ("Grammar") rather than the default underlying int, matching this codebase's "enums persist as their
/// string name" convention everywhere else (e.g. <c>ProgressTrackingConfiguration</c>'s
/// <c>HasConversion&lt;string&gt;()</c> on the same enum).
/// </summary>
internal static class LearningJsonOptions
{
    public static readonly JsonSerializerOptions Instance = new(JsonSerializerOptions.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
