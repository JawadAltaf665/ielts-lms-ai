using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Billing.Services;

/// <summary>
/// Application-layer implementation of the <see cref="IEntitlementService"/> port other bounded
/// contexts inject to gate feature access (13 §13, ADR-031). Resolution: TenantFeature override ??
/// PlanFeature grant on the tenant's current plan ?? deny.
/// </summary>
public sealed class EntitlementService(
    IFeatureRepository features,
    IPlanFeatureRepository planFeatures,
    ITenantFeatureRepository tenantFeatures,
    ISubscriptionRepository subscriptions,
    IClock clock)
    : IEntitlementService
{
    public async Task<bool> HasFeatureAsync(Guid tenantId, string featureCode, CancellationToken cancellationToken = default)
    {
        var feature = await features.GetByCodeAsync(featureCode, cancellationToken).ConfigureAwait(false);
        if (feature is null)
        {
            return false;
        }

        var overrideGrant = await tenantFeatures.GetAsync(tenantId, feature.Id, cancellationToken).ConfigureAwait(false);
        if (overrideGrant is not null && overrideGrant.IsCurrentlyEffective(clock.UtcNow))
        {
            return overrideGrant.IsEnabled;
        }

        var subscription = await subscriptions.GetCurrentByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return false;
        }

        var planGrant = await planFeatures.GetAsync(subscription.PlanId, feature.Id, cancellationToken).ConfigureAwait(false);
        return planGrant?.IsEnabled ?? false;
    }

    public async Task<IReadOnlyList<string>> ListEnabledFeatureCodesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var allFeatures = await features.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var enabled = new List<string>(allFeatures.Count);

        foreach (var feature in allFeatures)
        {
            if (await HasFeatureAsync(tenantId, feature.Code, cancellationToken).ConfigureAwait(false))
            {
                enabled.Add(feature.Code);
            }
        }

        return enabled;
    }
}
