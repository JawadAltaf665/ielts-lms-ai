namespace IELTSLms.Application.AI.GrammarCoach;

/// <summary>Structured Grammar Coach output (AI Architecture §8): a typed correction list, span +
/// error type + suggestion + explanation. Both the Claude adapter's tool schema and this query
/// handler's parsing agree on this exact shape.</summary>
public sealed record GrammarCoachOutput(IReadOnlyList<GrammarCorrection> Corrections);

public sealed record GrammarCorrection(string Span, string ErrorType, string Suggestion, string Explanation);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8) — provider-agnostic; a vendor adapter
/// translates it into whatever mechanism it uses (tool-use, JSON mode, ...).</summary>
internal static class GrammarCoachSchema
{
    public const string Name = "record_grammar_corrections";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "corrections": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "span": { "type": "string", "description": "The exact substring of the learner's text that contains the error." },
                  "errorType": { "type": "string", "description": "Short category, e.g. subject-verb-agreement, article, tense." },
                  "suggestion": { "type": "string", "description": "The corrected replacement text." },
                  "explanation": { "type": "string", "description": "A one- or two-sentence explanation aimed at a learner." }
                },
                "required": ["span", "errorType", "suggestion", "explanation"]
              }
            }
          },
          "required": ["corrections"]
        }
        """;
}
