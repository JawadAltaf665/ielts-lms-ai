using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking;

internal static class SpeakingErrors
{
    public static readonly Error TaskNotFound = Error.NotFound(
        "NOT_FOUND_SPEAKING_TASK", "Speaking task not found.");

    public static readonly Error TaskTitleTaken = Error.Conflict(
        "CONFLICT_SPEAKING_TASK_TITLE_TAKEN", "A speaking task with this title already exists.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_SPEAKING_ATTEMPT", "Speaking attempt not found.");

    public static readonly Error AttemptNotRecorded = Error.Conflict(
        "CONFLICT_SPEAKING_ATTEMPT_NOT_RECORDED", "This attempt is not in a submittable state.");

    public static readonly Error AttemptTooShort = Error.Validation(
        "VALIDATION_SPEAKING_ATTEMPT_TOO_SHORT", "The recording is shorter than the minimum required duration.");

    public static readonly Error EvaluationNotFound = Error.NotFound(
        "NOT_FOUND_SPEAKING_EVALUATION", "Speaking evaluation not found.");
}
