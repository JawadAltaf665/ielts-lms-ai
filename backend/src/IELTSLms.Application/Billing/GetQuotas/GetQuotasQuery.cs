using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetQuotas;

/// <summary>The tenant's effective quotas and current-period usage across the full quota catalog (13 §3).</summary>
public sealed record GetQuotasQuery(Guid TenantId) : IQuery<IReadOnlyList<QuotaStatusResponse>>;

public sealed record QuotaStatusResponse(string Code, string Name, string Unit, long Limit, long Consumed, long Remaining);
