using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Tenant-specific override/grant of a <see cref="Feature"/>, beyond or below the plan default
/// (ERD ENT-039G). Resolution order: TenantFeature override ?? PlanFeature ?? deny.
/// </summary>
public sealed class TenantFeature : TenantAuditableEntity<TenantFeatureId>
{
    private TenantFeature(
        TenantFeatureId id, Guid tenantId, FeatureId featureId, bool isEnabled, string? reason,
        DateTimeOffset? expiresAtUtc, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        FeatureId = featureId;
        IsEnabled = isEnabled;
        Reason = reason;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = nowUtc;
    }

    private TenantFeature()
    {
    }

    public FeatureId FeatureId { get; private set; }

    public bool IsEnabled { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public static TenantFeature Create(
        TenantFeatureId id, Guid tenantId, FeatureId featureId, bool isEnabled, string? reason,
        DateTimeOffset? expiresAtUtc, DateTimeOffset nowUtc) =>
        new(id, tenantId, featureId, isEnabled, reason, expiresAtUtc, nowUtc);

    public bool IsCurrentlyEffective(DateTimeOffset nowUtc) => ExpiresAtUtc is null || ExpiresAtUtc > nowUtc;
}
