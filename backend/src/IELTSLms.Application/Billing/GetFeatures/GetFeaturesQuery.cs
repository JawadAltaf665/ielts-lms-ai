using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetFeatures;

/// <summary>The tenant's effective feature entitlements across the full feature catalog (13 §13).</summary>
public sealed record GetFeaturesQuery(Guid TenantId) : IQuery<IReadOnlyList<FeatureAccessResponse>>;

public sealed record FeatureAccessResponse(string Code, string Name, string? FeatureGroupCode, bool Enabled);
