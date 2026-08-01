namespace IELTSLms.Application.AI.SpeakingExaminer;

/// <summary>
/// Structured Speaking Examiner output for the Speaking Evaluation Workflow (Agentic AI Architecture
/// §13.2): the four IELTS Speaking rubric criteria, a confidence score, and learner-facing feedback.
/// Mirrors <c>WritingExaminerOutput</c>; feedback composition is folded into this same call, same
/// merged-simplification note as Writing.
/// </summary>
public sealed record SpeakingExaminerOutput(
    decimal FluencyCoherence,
    decimal LexicalResource,
    decimal GrammaticalRangeAccuracy,
    decimal Pronunciation,
    decimal Confidence,
    string FeedbackText);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained — same mechanism as <c>WritingExaminerSchema</c>.</summary>
internal static class SpeakingExaminerSchema
{
    public const string Name = "record_speaking_examiner_scores";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "fluencyCoherence": { "type": "number", "description": "Fluency and Coherence band, 0-9 in 0.5 increments." },
            "lexicalResource": { "type": "number", "description": "Lexical Resource band, 0-9 in 0.5 increments." },
            "grammaticalRangeAccuracy": { "type": "number", "description": "Grammatical Range and Accuracy band, 0-9 in 0.5 increments." },
            "pronunciation": { "type": "number", "description": "Pronunciation band, 0-9 in 0.5 increments, inferred from the transcript." },
            "confidence": { "type": "number", "description": "The examiner's confidence in these scores, from 0 to 1." },
            "feedbackText": { "type": "string", "description": "Learner-facing feedback summarizing strengths and areas to improve." }
          },
          "required": [
            "fluencyCoherence", "lexicalResource", "grammaticalRangeAccuracy", "pronunciation", "confidence", "feedbackText"
          ]
        }
        """;
}
