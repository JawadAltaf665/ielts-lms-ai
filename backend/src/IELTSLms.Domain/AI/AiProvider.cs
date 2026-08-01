using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A registered AI/LLM provider and its routing config (ERD ENT-050). <see cref="TenantId"/> is
/// nullable: <see langword="null"/> for the global-catalog provider row every tenant resolves to by
/// default (mirrors <c>GrammarTopic</c>'s global-vs-tenant split), non-null for a future tenant-specific
/// override. Because of the nullable tenant this does not implement <see cref="ITenantScoped"/>, so it
/// is exempt from the automatic tenant query filter. Secrets (API keys) are never stored on this row —
/// they live in configuration/secret store, resolved at runtime by <see cref="Kind"/>.
/// </summary>
public sealed class AiProvider : AuditableEntity<AiProviderId>
{
    private AiProvider(AiProviderId id, Guid? tenantId, string name, AiProviderKind kind, string baseUrl, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Kind = kind;
        BaseUrl = baseUrl;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private AiProvider()
    {
        Name = null!;
        BaseUrl = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for the global-catalog provider.</summary>
    public Guid? TenantId { get; private init; }

    public string Name { get; private set; }

    public AiProviderKind Kind { get; private set; }

    public string BaseUrl { get; private set; }

    public bool IsActive { get; private set; }

    public static Result<AiProvider> Create(
        AiProviderId id, Guid? tenantId, string name, AiProviderKind kind, string baseUrl, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<AiProvider>(AiDomainErrors.ProviderNameRequired);
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return Result.Failure<AiProvider>(AiDomainErrors.ProviderBaseUrlRequired);
        }

        return Result.Success(new AiProvider(id, tenantId, name.Trim(), kind, baseUrl.Trim(), nowUtc));
    }

    public void Deactivate(DateTimeOffset nowUtc)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc;
    }
}

internal static class AiDomainErrors
{
    public static readonly Error ProviderNameRequired = Error.Validation(
        "VALIDATION_AI_PROVIDER_NAME_REQUIRED", "Provider name is required.");

    public static readonly Error ProviderBaseUrlRequired = Error.Validation(
        "VALIDATION_AI_PROVIDER_BASE_URL_REQUIRED", "Provider base URL is required.");

    public static readonly Error PromptFeatureRequired = Error.Validation(
        "VALIDATION_AI_PROMPT_FEATURE_REQUIRED", "Prompt feature is required.");

    public static readonly Error PromptSystemTemplateRequired = Error.Validation(
        "VALIDATION_AI_PROMPT_SYSTEM_TEMPLATE_REQUIRED", "Prompt system template is required.");

    public static readonly Error PromptVersionInvalid = Error.Validation(
        "VALIDATION_AI_PROMPT_VERSION_INVALID", "Prompt version must be positive.");

    public static readonly Error ResponseInputHashRequired = Error.Validation(
        "VALIDATION_AI_RESPONSE_INPUT_HASH_REQUIRED", "Response input hash is required.");
}
