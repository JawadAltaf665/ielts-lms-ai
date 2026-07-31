using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening;

internal static class ListeningErrors
{
    public static readonly Error AudioNotFound = Error.NotFound(
        "NOT_FOUND_LISTENING_AUDIO", "Listening audio not found.");

    public static readonly Error AudioTitleTaken = Error.Conflict(
        "CONFLICT_LISTENING_AUDIO_TITLE_TAKEN", "A listening audio with this title already exists.");

    public static readonly Error AudioReadOnly = Error.Forbidden(
        "AUTHZ_LISTENING_AUDIO_READ_ONLY", "Global listening audios are read-only.");

    public static readonly Error AudioFileMissing = Error.Failure(
        "LISTENING_AUDIO_FILE_MISSING", "This audio's file could not be located in storage.");

    public static readonly Error DuplicateQuestionSequence = Error.Validation(
        "VALIDATION_LISTENING_QUESTION_SEQUENCE_DUPLICATE", "A question with this sequence number already exists on this audio.");

    public static readonly Error NoQuestionsToAttempt = Error.Validation(
        "VALIDATION_LISTENING_AUDIO_EMPTY", "This audio has no questions to attempt.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_LISTENING_ATTEMPT", "Listening attempt not found.");

    public static readonly Error AttemptExpired = Error.Conflict(
        "CONFLICT_LISTENING_ATTEMPT_EXPIRED", "The time allowed for this attempt has expired.");
}
