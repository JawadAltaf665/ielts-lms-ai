namespace IELTSLms.Application.AI.WritingExaminer;

/// <summary>
/// Structured Writing Examiner output for the Writing Evaluation Workflow (Agentic AI Architecture
/// §13.1): the four IELTS rubric criteria, a confidence score, and learner-facing feedback. This one
/// call's <see cref="FeedbackText"/> also serves as the workflow's Feedback Composer step (merged
/// simplification — see the sprint's reconciliation notes) rather than a separate fourth AI call.
/// </summary>
public sealed record WritingExaminerOutput(
    decimal TaskResponse,
    decimal CoherenceCohesion,
    decimal LexicalResource,
    decimal GrammaticalRangeAccuracy,
    decimal Confidence,
    string FeedbackText,
    string Rationale);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained — same mechanism as <c>GrammarCoachSchema</c>.</summary>
internal static class WritingExaminerSchema
{
    public const string Name = "record_writing_examiner_scores";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "taskResponse": { "type": "number", "description": "Task Response band, 0-9 in 0.5 increments." },
            "coherenceCohesion": { "type": "number", "description": "Coherence and Cohesion band, 0-9 in 0.5 increments." },
            "lexicalResource": { "type": "number", "description": "Lexical Resource band, 0-9 in 0.5 increments." },
            "grammaticalRangeAccuracy": { "type": "number", "description": "Grammatical Range and Accuracy band, 0-9 in 0.5 increments." },
            "confidence": { "type": "number", "description": "The examiner's confidence in these scores, from 0 to 1." },
            "feedbackText": { "type": "string", "description": "Learner-facing feedback summarizing strengths and areas to improve." },
            "rationale": { "type": "string", "description": "A short examiner-facing rationale explaining how each band was reached." }
          },
          "required": [
            "taskResponse", "coherenceCohesion", "lexicalResource", "grammaticalRangeAccuracy",
            "confidence", "feedbackText", "rationale"
          ]
        }
        """;
}
