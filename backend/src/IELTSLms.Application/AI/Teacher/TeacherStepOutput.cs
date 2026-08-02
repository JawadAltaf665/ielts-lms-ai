using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Teacher;

/// <summary>
/// Structured AI Teacher step output (Phase 7 Sprint 4), shared across all four
/// <see cref="Domain.AI.LessonStep"/> prompts. <see cref="Passed"/> is only populated (and only
/// meaningful) for the <c>check-understanding</c> step's schema — the other three steps' schemas omit
/// it, so it deserializes as <see langword="null"/> for them. One shared C# shape avoids four near-
/// identical records for what is really one "content the learner sees, plus an optional pass signal" idea.
/// </summary>
public sealed record TeacherStepOutput(string Content, bool? Passed);

/// <summary>The JSON Schemas handed to <see cref="Abstractions.AI.IAiGateway"/> per lesson step
/// (AI Architecture §8) — one per <see cref="Domain.AI.LessonStep"/>, selected by the step-state
/// machine, per the sprint's own framing of why prompts are scoped per step rather than shared.</summary>
internal static class TeacherStepSchemas
{
    public const string ExplainName = "record_teacher_explain_step";
    public const string ExplainJson = """
        {
          "type": "object",
          "properties": {
            "content": { "type": "string", "description": "A clear explanation of the lesson topic, aimed at an IELTS learner." }
          },
          "required": ["content"]
        }
        """;

    public const string PracticeName = "record_teacher_practice_step";
    public const string PracticeJson = """
        {
          "type": "object",
          "properties": {
            "content": { "type": "string", "description": "A practice exercise or prompt for the learner to attempt, related to the lesson topic." }
          },
          "required": ["content"]
        }
        """;

    public const string RecapName = "record_teacher_recap_step";
    public const string RecapJson = """
        {
          "type": "object",
          "properties": {
            "content": { "type": "string", "description": "A short recap of the lesson's key points and a closing note for the learner." }
          },
          "required": ["content"]
        }
        """;

    public const string CheckUnderstandingName = "record_teacher_check_understanding_step";
    public const string CheckUnderstandingJson = """
        {
          "type": "object",
          "properties": {
            "content": { "type": "string", "description": "Feedback on the learner's answer to the comprehension check, or the comprehension question itself if none has been asked yet this turn." },
            "passed": { "type": "boolean", "description": "Whether the learner's response demonstrates understanding of the lesson topic." }
          },
          "required": ["content", "passed"]
        }
        """;

    public static (string Name, string Json) ForStep(LessonStep step) => step switch
    {
        LessonStep.Explain => (ExplainName, ExplainJson),
        LessonStep.CheckUnderstanding => (CheckUnderstandingName, CheckUnderstandingJson),
        LessonStep.Practice => (PracticeName, PracticeJson),
        LessonStep.Recap => (RecapName, RecapJson),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Completed has no step prompt/schema."),
    };
}
