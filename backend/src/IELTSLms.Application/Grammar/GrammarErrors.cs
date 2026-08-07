using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar;

internal static class GrammarErrors
{
    public static readonly Error TopicNotFound = Error.NotFound(
        "NOT_FOUND_GRAMMAR_TOPIC", "Grammar topic not found.");

    public static readonly Error TopicTitleTaken = Error.Conflict(
        "CONFLICT_GRAMMAR_TOPIC_TITLE_TAKEN", "A grammar topic with this title already exists.");

    public static readonly Error TopicReadOnly = Error.Forbidden(
        "AUTHZ_GRAMMAR_TOPIC_READ_ONLY", "Global grammar topics are read-only.");

    public static readonly Error ExerciseNotFound = Error.NotFound(
        "NOT_FOUND_GRAMMAR_EXERCISE", "Grammar exercise not found.");

    public static readonly Error DuplicateQuestionSequence = Error.Validation(
        "VALIDATION_GRAMMAR_QUESTION_SEQUENCE_DUPLICATE", "Question sequence numbers must be unique within an exercise.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_GRAMMAR_ATTEMPT", "Grammar attempt not found.");

    public static readonly Error NoQuestionsToGrade = Error.Validation(
        "VALIDATION_GRAMMAR_EXERCISE_EMPTY", "This exercise has no questions to grade.");

    public static readonly Error NoIncorrectAnswers = Error.Validation(
        "VALIDATION_GRAMMAR_ATTEMPT_NO_ERRORS", "This attempt has no incorrect answers to coach on.");
}
