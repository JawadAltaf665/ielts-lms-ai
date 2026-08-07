namespace IELTSLms.Application.Learning.WeaknessDetection;

/// <summary>Structured Progress Analysis output (Agentic AI Architecture §12.2/§13.3): a weakness score
/// per skill, always covering all six of <see cref="ProgressAnalysisSchema.RelevantSkills"/>.</summary>
public sealed record ProgressAnalysisOutput(IReadOnlyList<ProgressAnalysisSkillScore> Scores);

/// <param name="WeaknessScore">0-100, higher = weaker.</param>
public sealed record ProgressAnalysisSkillScore(string Skill, decimal WeaknessScore, string Note);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8).</summary>
internal static class ProgressAnalysisSchema
{
    public const string Name = "record_progress_analysis";

    public static readonly string[] RelevantSkills = ["Grammar", "Vocabulary", "Reading", "Listening", "Writing", "Speaking"];

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "scores": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "skill": { "type": "string", "enum": ["Grammar", "Vocabulary", "Reading", "Listening", "Writing", "Speaking"] },
                  "weaknessScore": { "type": "number", "description": "0-100, higher = weaker." },
                  "note": { "type": "string", "description": "One-sentence explanation of the score." }
                },
                "required": ["skill", "weaknessScore", "note"]
              }
            }
          },
          "required": ["scores"]
        }
        """;
}
