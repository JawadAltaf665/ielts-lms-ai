namespace IELTSLms.Application.Learning.StudyPlanner;

/// <summary>Structured Study Planner output (Agentic AI Architecture §12.3/§13.3): today's task list,
/// turned into <c>DailyPlanItemDto</c> rows (each assigned an id by the caller, not the model).</summary>
public sealed record StudyPlannerOutput(IReadOnlyList<StudyPlannerItem> Items);

public sealed record StudyPlannerItem(string Skill, string Title, string Description, int DurationMinutes);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8).</summary>
internal static class StudyPlannerSchema
{
    public const string Name = "record_daily_study_plan";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "maxItems": 6,
              "items": {
                "type": "object",
                "properties": {
                  "skill": { "type": "string", "enum": ["Grammar", "Vocabulary", "Reading", "Listening", "Writing", "Speaking"] },
                  "title": { "type": "string" },
                  "description": { "type": "string" },
                  "durationMinutes": { "type": "integer", "description": "5-45 minutes." }
                },
                "required": ["skill", "title", "description", "durationMinutes"]
              }
            }
          },
          "required": ["items"]
        }
        """;
}
