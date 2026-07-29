using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Grants a <see cref="Feature"/> to a <see cref="Plan"/> — the entitlement matrix (ERD ENT-039F).
/// A feature absent from a plan's grants is denied by default.
/// </summary>
public sealed class PlanFeature : AuditableEntity<PlanFeatureId>
{
    private PlanFeature(PlanFeatureId id, PlanId planId, FeatureId featureId, bool isEnabled, DateTimeOffset nowUtc)
        : base(id)
    {
        PlanId = planId;
        FeatureId = featureId;
        IsEnabled = isEnabled;
        CreatedAtUtc = nowUtc;
    }

    private PlanFeature()
    {
    }

    public PlanId PlanId { get; private set; }

    public FeatureId FeatureId { get; private set; }

    public bool IsEnabled { get; private set; }

    public static PlanFeature Create(PlanFeatureId id, PlanId planId, FeatureId featureId, bool isEnabled, DateTimeOffset nowUtc) =>
        new(id, planId, featureId, isEnabled, nowUtc);

    public void SetEnabled(bool isEnabled, DateTimeOffset nowUtc)
    {
        IsEnabled = isEnabled;
        UpdatedAtUtc = nowUtc;
    }
}
