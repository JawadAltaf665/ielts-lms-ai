namespace IELTSLms.Application.AI.ReadingCoach;

/// <summary>Structured Reading Coach output (AI Architecture §13 capability catalog, Sprint 8): an
/// explanation grounded in the passage — used both for a specific wrong answer
/// (<c>GetReadingAttemptCoachingQuery</c>) and for general on-demand strategy guidance
/// (<c>RequestReadingCoachingCommand</c>), where <see cref="Explanation"/> answers the learner's
/// question directly. Both the Claude adapter's tool schema and this query handler's parsing agree on
/// this exact shape — mirrors <c>GrammarCoachOutput</c>.</summary>
public sealed record ReadingCoachOutput(string Explanation, IReadOnlyList<string> StrategyTips);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained and stays grounded in the supplied passage/question text
/// (no hallucinated content beyond it, per Sprint 8's testing section) — same mechanism as
/// <c>GrammarCoachSchema</c>.</summary>
internal static class ReadingCoachSchema
{
    public const string Name = "record_reading_coaching";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "explanation": {
              "type": "string",
              "description": "An explanation grounded strictly in the supplied passage/question text — never introduce facts not present in it."
            },
            "strategyTips": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Short, actionable skimming/scanning or question-type strategy tips relevant to the input."
            }
          },
          "required": ["explanation", "strategyTips"]
        }
        """;
}
