using IELTSLms.Domain.Tenancy.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// The tenant aggregate root: the isolation boundary that owns all tenant-scoped data (ERD ENT-001,
/// Multi-Tenant Architecture §2). It enforces its lifecycle invariants through methods rather than
/// open setters (Development Standards §15, DDD). It is the one entity that is <em>not</em> itself
/// tenant-scoped — it is the root every other aggregate's <c>TenantId</c> points at.
/// </summary>
public sealed class Tenant : AggregateRoot<TenantId>, IAuditableEntity, ISoftDeletable
{
    private Tenant(
        TenantId id,
        TenantSlug slug,
        TenantCode code,
        string name,
        string? primaryDomain,
        string? contactEmail,
        string defaultLocale,
        string defaultTimeZone,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Slug = slug;
        Code = code;
        Name = name;
        PrimaryDomain = primaryDomain;
        ContactEmail = contactEmail;
        DefaultLocale = defaultLocale;
        DefaultTimeZone = defaultTimeZone;
        Status = TenantStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Tenant()
    {
        // EF Core materialization.
        Slug = null!;
        Code = null!;
        Name = null!;
        DefaultLocale = null!;
        DefaultTimeZone = null!;
    }

    /// <summary>Default locale applied when a tenant sets none (BCP-47, e.g. <c>en-US</c>).</summary>
    public const string FallbackLocale = "en-US";

    /// <summary>Default IANA time zone applied when a tenant sets none.</summary>
    public const string FallbackTimeZone = "UTC";

    /// <summary>Maximum stored length for the display name.</summary>
    public const int NameMaxLength = 256;

    /// <summary>Maximum stored length for the primary domain.</summary>
    public const int DomainMaxLength = 253;

    /// <summary>Maximum stored length for the contact email.</summary>
    public const int ContactEmailMaxLength = 320;

    /// <summary>Maximum stored length for locale/time-zone identifiers.</summary>
    public const int LocaleMaxLength = 64;

    /// <summary>URL-safe, globally unique slug; backs host-based resolution and references.</summary>
    public TenantSlug Slug { get; private set; }

    /// <summary>Short, globally unique human-facing code (e.g. <c>ACME</c>).</summary>
    public TenantCode Code { get; private set; }

    /// <summary>Display name of the tenant/organization.</summary>
    public string Name { get; private set; }

    /// <summary>Optional custom primary domain used for branded resolution, or <see langword="null"/>.</summary>
    public string? PrimaryDomain { get; private set; }

    /// <summary>Optional administrative contact email, or <see langword="null"/>.</summary>
    public string? ContactEmail { get; private set; }

    /// <summary>Default locale for the tenant (BCP-47).</summary>
    public string DefaultLocale { get; private set; }

    /// <summary>Default IANA time zone for the tenant.</summary>
    public string DefaultTimeZone { get; private set; }

    /// <summary>Operational lifecycle state (ERD ENT-001).</summary>
    public TenantStatus Status { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin) maintained by PostgreSQL (Backend Architecture §9).</summary>
    public uint Version { get; private set; }

    /// <summary>
    /// Derived operational readiness: a tenant is usable only when it is <see cref="TenantStatus.Active"/>
    /// and not soft-deleted. Not stored — computed from <see cref="Status"/> and <see cref="IsDeleted"/>
    /// so the two can never drift (Sprint 3 resolution, ERD-aligned).
    /// </summary>
    public bool IsActive => Status is TenantStatus.Active && !IsDeleted;

    /// <summary>
    /// Provisions a new tenant in the active state and raises <see cref="TenantCreatedDomainEvent"/>.
    /// Slug and code uniqueness are checked by the application layer before calling and enforced by
    /// the database. Locale/time-zone fall back to platform defaults when not supplied.
    /// </summary>
    public static Tenant Create(
        TenantId id,
        TenantSlug slug,
        TenantCode code,
        string name,
        DateTimeOffset nowUtc,
        string? primaryDomain = null,
        string? contactEmail = null,
        string? defaultLocale = null,
        string? defaultTimeZone = null)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var tenant = new Tenant(
            id,
            slug,
            code,
            name.Trim(),
            Normalize(primaryDomain),
            Normalize(contactEmail),
            Coalesce(defaultLocale, FallbackLocale),
            Coalesce(defaultTimeZone, FallbackTimeZone),
            nowUtc);

        tenant.RaiseDomainEvent(new TenantCreatedDomainEvent(id.Value, slug.Value, code.Value));
        return tenant;
    }

    /// <summary>
    /// Updates the mutable profile (name, primary domain, contact, locale defaults). Slug and code are
    /// stable identifiers and are not changed here. Raises <see cref="TenantUpdatedDomainEvent"/>.
    /// </summary>
    public Result UpdateProfile(
        string name,
        string? primaryDomain,
        string? contactEmail,
        string? defaultLocale,
        string? defaultTimeZone,
        DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return Result.Failure(Error.Conflict("CONFLICT_TENANT_DELETED", "A closed tenant cannot be modified."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("VALIDATION_REQUIRED", "Tenant name is required."));
        }

        Name = name.Trim();
        PrimaryDomain = Normalize(primaryDomain);
        ContactEmail = Normalize(contactEmail);
        DefaultLocale = Coalesce(defaultLocale, FallbackLocale);
        DefaultTimeZone = Coalesce(defaultTimeZone, FallbackTimeZone);
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new TenantUpdatedDomainEvent(Id.Value));
        return Result.Success();
    }

    /// <summary>
    /// Reinstates a suspended tenant to the active state. Idempotent when already active; a closed
    /// (soft-deleted) tenant must be restored first. Raises <see cref="TenantActivatedDomainEvent"/>.
    /// </summary>
    public Result Activate(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return Result.Failure(Error.Conflict("CONFLICT_TENANT_DELETED", "A closed tenant must be restored before it can be activated."));
        }

        if (Status is TenantStatus.Active)
        {
            return Result.Success();
        }

        Status = TenantStatus.Active;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new TenantActivatedDomainEvent(Id.Value));
        return Result.Success();
    }

    /// <summary>
    /// Suspends an active tenant (non-payment, policy hold, admin action). Access is denied while
    /// suspended but data is retained. Idempotent when already suspended.
    /// Raises <see cref="TenantDeactivatedDomainEvent"/>.
    /// </summary>
    public Result Deactivate(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return Result.Failure(Error.Conflict("CONFLICT_TENANT_DELETED", "A closed tenant cannot be suspended."));
        }

        if (Status is TenantStatus.Suspended)
        {
            return Result.Success();
        }

        Status = TenantStatus.Suspended;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new TenantDeactivatedDomainEvent(Id.Value));
        return Result.Success();
    }

    /// <summary>
    /// Soft-deletes (closes) the tenant: sets <see cref="TenantStatus.Closed"/> and the soft-delete
    /// flags. Data is retained for the retention window (ERD ENT-001). Idempotent.
    /// Raises <see cref="TenantDeletedDomainEvent"/>.
    /// </summary>
    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        Status = TenantStatus.Closed;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new TenantDeletedDomainEvent(Id.Value));
    }

    /// <summary>
    /// Restores a soft-deleted tenant within its retention window, returning it to the suspended state
    /// so an operator explicitly re-activates it. Idempotent when not deleted.
    /// Raises <see cref="TenantRestoredDomainEvent"/>.
    /// </summary>
    public Result Restore(DateTimeOffset nowUtc)
    {
        if (!IsDeleted)
        {
            return Result.Success();
        }

        IsDeleted = false;
        DeletedAtUtc = null;
        Status = TenantStatus.Suspended;
        UpdatedAtUtc = nowUtc;
        RaiseDomainEvent(new TenantRestoredDomainEvent(Id.Value));
        return Result.Success();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Coalesce(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
