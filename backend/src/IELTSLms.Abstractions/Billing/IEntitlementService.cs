namespace IELTSLms.Abstractions.Billing;

/// <summary>
/// Resolves effective feature entitlements for a tenant (13 §13, DDD §"EntitlementService"). Other
/// bounded contexts depend on this port — never on Billing's persistence — to gate feature access.
/// Resolution order: TenantFeature override ?? PlanFeature grant on the tenant's current plan ?? deny.
/// </summary>
public interface IEntitlementService
{
    /// <summary>Whether the tenant currently has access to the given feature code.</summary>
    Task<bool> HasFeatureAsync(Guid tenantId, string featureCode, CancellationToken cancellationToken = default);

    /// <summary>All feature codes currently enabled for the tenant.</summary>
    Task<IReadOnlyList<string>> ListEnabledFeatureCodesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
