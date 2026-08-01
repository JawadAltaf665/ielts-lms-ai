using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI;

internal static class AiErrors
{
    public static readonly Error FeatureNotEntitled = Error.Forbidden(
        "AUTHZ_AI_FEATURE_NOT_ENTITLED", "Your plan does not include this AI capability.");

    public static readonly Error QuotaExceeded = Error.RateLimited(
        "RATE_LIMITED_AI_QUOTA_EXCEEDED", "You have exceeded your AI usage quota for the current period.");

    public static readonly Error ProviderUnavailable = Error.ServiceUnavailable(
        "DEPENDENCY_AI_UNAVAILABLE", "The AI service is temporarily unavailable. Please try again shortly.");

    public static readonly Error InputRejected = Error.Validation(
        "VALIDATION_AI_INPUT_REJECTED", "The submitted text could not be processed.");

    public static readonly Error OperationNotFound = Error.NotFound(
        "NOT_FOUND_AI_OPERATION", "AI operation not found.");

    public static readonly Error PromptNotConfigured = Error.ServiceUnavailable(
        "DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", "This AI capability is not yet configured.");
}
