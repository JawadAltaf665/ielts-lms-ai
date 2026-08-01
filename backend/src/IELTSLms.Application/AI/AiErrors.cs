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

    public static readonly Error PromptVariableMissing = Error.Validation(
        "VALIDATION_AI_PROMPT_VARIABLE_MISSING", "The prompt template references a variable that was not supplied.");

    public static readonly Error PromptVariableInjectionRejected = Error.Validation(
        "VALIDATION_AI_PROMPT_VARIABLE_INJECTION_REJECTED",
        "A supplied prompt variable was rejected because it contained template syntax.");

    public static readonly Error UsageSummaryInvalidRange = Error.Validation(
        "VALIDATION_AI_USAGE_SUMMARY_INVALID_RANGE", "The 'from' date must not be after the 'to' date.");

    public static readonly Error PromptNotFound = Error.NotFound(
        "NOT_FOUND_AI_PROMPT", "Prompt version not found.");

    public static readonly Error PromptVersionMismatch = Error.Conflict(
        "CONFLICT_AI_PROMPT_VERSION_MISMATCH", "The prompt was modified by someone else since you loaded it. Reload and try again.");

    public static readonly Error PromptNotActive = Error.Validation(
        "BUSINESS_AI_PROMPT_NOT_ACTIVE", "Only the currently active version for its feature/scope can be rolled back.");

    public static readonly Error PromptNoPriorVersion = Error.Validation(
        "BUSINESS_AI_PROMPT_NO_PRIOR_VERSION", "There is no prior version to roll back to for this feature/scope.");

    public static readonly Error PromptActiveCannotRetire = Error.Validation(
        "BUSINESS_AI_PROMPT_ACTIVE_CANNOT_RETIRE",
        "This prompt version is currently active. Activate a replacement before retiring it.");
}
