namespace IELTSLms.Application.AI.SentenceBuilderAi;

/// <summary>Structured Sentence Builder AI output (Sprint 7): whether the learner's sentence matches
/// the exercise's target pattern, feedback, and — when it doesn't match — a suggested revision.</summary>
public sealed record SentenceBuilderAiOutput(bool MatchesPattern, string Feedback, string? SuggestedRevision);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained — same mechanism as <c>GrammarCoachSchema</c>.</summary>
internal static class SentenceBuilderAiSchema
{
    public const string Name = "record_sentence_builder_feedback";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "matchesPattern": { "type": "boolean", "description": "Whether the sentence correctly uses the target pattern." },
            "feedback": { "type": "string", "description": "Brief, constructive feedback for the learner." },
            "suggestedRevision": { "type": ["string", "null"], "description": "A corrected sentence, only when matchesPattern is false." }
          },
          "required": ["matchesPattern", "feedback", "suggestedRevision"]
        }
        """;
}
