using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Analytics;

internal static class AnalyticsErrors
{
    public static readonly Error InvalidAttemptType = Error.Validation(
        "VALIDATION_ATTEMPT_TYPE_INVALID",
        "'{type}' must be one of writing, speaking, reading, listening, grammar, vocabulary, mock-exam.");

    public static readonly Error AttemptNotFound = Error.NotFound(
        "NOT_FOUND_ATTEMPT", "Attempt not found.");
}
