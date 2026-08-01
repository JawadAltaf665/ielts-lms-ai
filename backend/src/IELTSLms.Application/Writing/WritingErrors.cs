using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing;

internal static class WritingErrors
{
    public static readonly Error TaskNotFound = Error.NotFound(
        "NOT_FOUND_WRITING_TASK", "Writing task not found.");

    public static readonly Error TaskTitleTaken = Error.Conflict(
        "CONFLICT_WRITING_TASK_TITLE_TAKEN", "A writing task with this title already exists.");

    public static readonly Error SubmissionNotFound = Error.NotFound(
        "NOT_FOUND_WRITING_SUBMISSION", "Writing submission not found.");

    public static readonly Error WordCountOutOfBounds = Error.Validation(
        "VALIDATION_WRITING_WORD_COUNT_OUT_OF_BOUNDS", "The response's word count is outside the task's configured bounds.");

    public static readonly Error EvaluationNotFound = Error.NotFound(
        "NOT_FOUND_WRITING_EVALUATION", "Writing evaluation not found.");
}
