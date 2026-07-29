using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetFeatures;

/// <summary>
/// Resolves the full feature catalog for the tenant: TenantFeature override ?? PlanFeature grant on
/// the tenant's current plan ?? deny (13 §13). Lives alongside <see cref="Services.EntitlementService"/>
/// but returns the full catalog (for a UI feature matrix) rather than a single yes/no check.
/// </summary>
public sealed class GetFeaturesQueryHandler(
    IFeatureRepository features,
    IFeatureGroupRepository featureGroups,
    IPlanFeatureRepository planFeatures,
    ITenantFeatureRepository tenantFeatures,
    ISubscriptionRepository subscriptions,
    IClock clock)
    : IQueryHandler<GetFeaturesQuery, IReadOnlyList<FeatureAccessResponse>>
{
    public async Task<Result<IReadOnlyList<FeatureAccessResponse>>> Handle(
        GetFeaturesQuery request, CancellationToken cancellationToken)
    {
        var allFeatures = await features.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var allGroups = await featureGroups.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var groupCodesById = allGroups.ToDictionary(g => g.Id, g => g.Code);

        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var planGrants = subscription is null
            ? []
            : (await planFeatures.ListByPlanAsync(subscription.PlanId, cancellationToken).ConfigureAwait(false))
                .Where(pf => pf.IsEnabled)
                .Select(pf => pf.FeatureId)
                .ToHashSet();

        var overrides = (await tenantFeatures.ListByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false))
            .Where(tf => tf.IsCurrentlyEffective(clock.UtcNow))
            .ToDictionary(tf => tf.FeatureId, tf => tf.IsEnabled);

        IReadOnlyList<FeatureAccessResponse> responses = allFeatures
            .Select(f => new FeatureAccessResponse(
                f.Code,
                f.Name,
                f.FeatureGroupId is { } groupId && groupCodesById.TryGetValue(groupId, out var code) ? code : null,
                overrides.TryGetValue(f.Id, out var overrideEnabled) ? overrideEnabled : planGrants.Contains(f.Id)))
            .ToArray();

        return Result.Success(responses);
    }
}
