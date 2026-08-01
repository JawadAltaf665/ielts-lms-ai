namespace IELTSLms.Application.AI.VocabularyAnalysis;

/// <summary>Structured Vocabulary Agent output for the Writing Evaluation Workflow (Agentic AI
/// Architecture §13.1): lexical issues found in a submission, plus an overall lexical-variety note.
/// Feeds into the Writing Examiner call as supporting context for Lexical Resource scoring.</summary>
public sealed record VocabularyAnalysisOutput(IReadOnlyList<VocabularyIssue> Issues, string LexicalVarietyNote);

public sealed record VocabularyIssue(string Span, string IssueType, string Suggestion, string Explanation);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained — same mechanism as <c>GrammarCoachSchema</c>.</summary>
internal static class VocabularyAnalysisSchema
{
    public const string Name = "record_vocabulary_analysis";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "issues": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "span": { "type": "string", "description": "The exact substring of the learner's text containing the lexical issue." },
                  "issueType": { "type": "string", "description": "Short category, e.g. repetition, wrong-register, wrong-collocation, informal." },
                  "suggestion": { "type": "string", "description": "A more precise or varied word/phrase choice." },
                  "explanation": { "type": "string", "description": "A one- or two-sentence explanation aimed at a learner." }
                },
                "required": ["span", "issueType", "suggestion", "explanation"]
              }
            },
            "lexicalVarietyNote": { "type": "string", "description": "One or two sentences summarizing overall vocabulary range and precision." }
          },
          "required": ["issues", "lexicalVarietyNote"]
        }
        """;
}
