namespace IELTSLms.Application.Learning.Recommendations;

/// <summary>Structured Learning Recommendation output (Agentic AI Architecture §12.3/§13.7): up to 5
/// ranked next actions.</summary>
public sealed record LearningRecommendationOutput(IReadOnlyList<LearningRecommendationItem> Items);

/// <param name="Priority">1 = highest priority.</param>
public sealed record LearningRecommendationItem(string Skill, string Title, string Rationale, int Priority);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8).</summary>
internal static class LearningRecommendationSchema
{
    public const string Name = "record_learning_recommendations";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "maxItems": 5,
              "items": {
                "type": "object",
                "properties": {
                  "skill": { "type": "string", "enum": ["Grammar", "Vocabulary", "Reading", "Listening", "Writing", "Speaking"] },
                  "title": { "type": "string" },
                  "rationale": { "type": "string", "description": "One sentence, grounded in the weakness profile." },
                  "priority": { "type": "integer", "description": "1 = highest priority." }
                },
                "required": ["skill", "title", "rationale", "priority"]
              }
            }
          },
          "required": ["items"]
        }
        """;
}
