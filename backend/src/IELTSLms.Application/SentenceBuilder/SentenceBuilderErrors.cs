using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder;

internal static class SentenceBuilderErrors
{
    public static readonly Error ExerciseNotFound = Error.NotFound(
        "NOT_FOUND_SENTENCE_EXERCISE", "Sentence exercise not found.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_SENTENCE_ATTEMPT", "Sentence attempt not found.");
}
