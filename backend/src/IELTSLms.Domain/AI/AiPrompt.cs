using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A versioned prompt template for an AI capability (ERD ENT-051, AI Architecture §5). Versioned and
/// immutable once created — there is deliberately no <c>Update</c> method; a prompt change is always a
/// new row with an incremented <see cref="PromptVersion"/> so every <see cref="AiResponse"/> can record
/// the exact template that produced it (reproducibility). Named <c>PromptVersion</c> rather than
/// <c>Version</c> to avoid colliding with <see cref="AuditableEntity{TId}.Version"/>, the unrelated
/// optimistic-concurrency token inherited from the base class. <see cref="TenantId"/> nullable mirrors
/// <see cref="AiProvider"/>'s global-vs-tenant split — not <see cref="ITenantScoped"/>.
/// <para>
/// Phase 7 Sprint 2 (Prompt Library &amp; Versioning): a newly created version is a <b>draft</b> — it
/// does not go live until a separate <see cref="Activate"/> call promotes it (the admin rollout/A-B
/// slot-assignment point). This keeps authoring and rollout as two governed, independently-audited
/// steps rather than one.
/// </para>
/// </summary>
public sealed class AiPrompt : AuditableEntity<AiPromptId>
{
    private AiPrompt(
        AiPromptId id, Guid? tenantId, AiProviderId? aiProviderId, string feature, int version, string scope,
        string systemTemplate, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        AiProviderId = aiProviderId;
        Feature = feature;
        PromptVersion = version;
        Scope = scope;
        SystemTemplate = systemTemplate;
        IsActive = false;
        CreatedAtUtc = nowUtc;
    }

    private AiPrompt()
    {
        Feature = null!;
        Scope = null!;
        SystemTemplate = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for the global-default prompt.</summary>
    public Guid? TenantId { get; private init; }

    public AiProviderId? AiProviderId { get; private init; }

    /// <summary>Capability tag, e.g. <c>"grammar-coach"</c>.</summary>
    public string Feature { get; private init; }

    public int PromptVersion { get; private init; }

    /// <summary>Distinguishes the global default from a tenant-specific override, e.g. <c>"default"</c>.</summary>
    public string Scope { get; private init; }

    public string SystemTemplate { get; private init; }

    public bool IsActive { get; private set; }

    public static Result<AiPrompt> Create(
        AiPromptId id, Guid? tenantId, AiProviderId? aiProviderId, string feature, int version, string scope,
        string systemTemplate, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(feature))
        {
            return Result.Failure<AiPrompt>(AiDomainErrors.PromptFeatureRequired);
        }

        if (version <= 0)
        {
            return Result.Failure<AiPrompt>(AiDomainErrors.PromptVersionInvalid);
        }

        if (string.IsNullOrWhiteSpace(systemTemplate))
        {
            return Result.Failure<AiPrompt>(AiDomainErrors.PromptSystemTemplateRequired);
        }

        return Result.Success(new AiPrompt(
            id, tenantId, aiProviderId, feature.Trim(), version, string.IsNullOrWhiteSpace(scope) ? "default" : scope.Trim(),
            systemTemplate, nowUtc));
    }

    /// <summary>Promotes this version to live for its (TenantId, Feature, Scope) slot. Callers are
    /// responsible for deactivating whatever else was active in the same slot — this method only
    /// flips this row (mirrors <see cref="Deactivate"/>, kept symmetric and side-effect-free).</summary>
    public void Activate(DateTimeOffset nowUtc)
    {
        IsActive = true;
        UpdatedAtUtc = nowUtc;
    }

    public void Deactivate(DateTimeOffset nowUtc)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc;
    }

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
