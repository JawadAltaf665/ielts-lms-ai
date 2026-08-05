namespace IELTSLms.Application.AI.ListeningCoach;

/// <summary>Structured Listening Coach output (AI Architecture §13 capability catalog, Sprint 8) —
/// mirrors <c>ReadingCoachOutput</c> exactly, grounded in the audio's transcript instead of a passage.</summary>
public sealed record ListeningCoachOutput(string Explanation, IReadOnlyList<string> StrategyTips);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> — mirrors
/// <c>ReadingCoachSchema</c> exactly.</summary>
internal static class ListeningCoachSchema
{
    public const string Name = "record_listening_coaching";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "explanation": {
              "type": "string",
              "description": "An explanation grounded strictly in the supplied transcript/question text — never introduce facts not present in it."
            },
            "strategyTips": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Short, actionable listening-strategy or question-type tips relevant to the input."
            }
          },
          "required": ["explanation", "strategyTips"]
        }
        """;
}
