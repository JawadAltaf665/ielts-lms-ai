using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading;

internal static class ReadingErrors
{
    public static readonly Error PassageNotFound = Error.NotFound(
        "NOT_FOUND_READING_PASSAGE", "Reading passage not found.");

    public static readonly Error PassageTitleTaken = Error.Conflict(
        "CONFLICT_READING_PASSAGE_TITLE_TAKEN", "A reading passage with this title already exists.");

    public static readonly Error PassageReadOnly = Error.Forbidden(
        "AUTHZ_READING_PASSAGE_READ_ONLY", "Global reading passages are read-only.");

    public static readonly Error DuplicateQuestionSequence = Error.Validation(
        "VALIDATION_READING_QUESTION_SEQUENCE_DUPLICATE", "A question with this sequence number already exists on this passage.");

    public static readonly Error NoQuestionsToAttempt = Error.Validation(
        "VALIDATION_READING_PASSAGE_EMPTY", "This passage has no questions to attempt.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_READING_ATTEMPT", "Reading attempt not found.");

    public static readonly Error AttemptExpired = Error.Conflict(
        "CONFLICT_READING_ATTEMPT_EXPIRED", "The time allowed for this attempt has expired.");
}
