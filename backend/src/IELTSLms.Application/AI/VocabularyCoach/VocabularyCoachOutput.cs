namespace IELTSLms.Application.AI.VocabularyCoach;

/// <summary>Structured Vocabulary Coach output (AI Architecture §13 capability catalog): usage
/// examples, collocations, and synonym suggestions for a word. Both the Claude adapter's tool schema
/// and this query handler's parsing agree on this exact shape — mirrors <c>GrammarCoachOutput</c>.</summary>
public sealed record VocabularyCoachOutput(
    IReadOnlyList<string> UsageExamples, IReadOnlyList<string> Collocations, IReadOnlyList<string> Synonyms);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained — same mechanism as <c>GrammarCoachSchema</c>.</summary>
internal static class VocabularyCoachSchema
{
    public const string Name = "record_vocabulary_coaching";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "usageExamples": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Natural, IELTS-relevant example sentences using the word."
            },
            "collocations": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Common word pairings/phrases the word typically appears in."
            },
            "synonyms": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Synonym suggestions appropriate to the given difficulty."
            }
          },
          "required": ["usageExamples", "collocations", "synonyms"]
        }
        """;
}
