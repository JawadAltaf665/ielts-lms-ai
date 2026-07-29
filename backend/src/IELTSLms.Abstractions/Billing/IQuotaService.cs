using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Abstractions.Billing;

/// <summary>
/// Resolves and enforces effective usage quotas for a tenant (13 §3, §13). Other bounded contexts
/// depend on this port — never on Billing's persistence — to check and record metered usage.
/// Resolution order: TenantQuota override ?? PlanQuota on the tenant's current plan ?? 0.
/// </summary>
public interface IQuotaService
{
    /// <summary>Checks whether consuming <paramref name="amount"/> of the given quota would stay within the tenant's limit.</summary>
    Task<QuotaCheckResult> CheckAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default);

    /// <summary>Records consumption against the tenant's current usage period. Fails if it would exceed the limit.</summary>
    Task<Result> ConsumeAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default);
}

/// <summary>Outcome of a quota availability check.</summary>
/// <param name="Allowed">Whether the requested amount fits within the remaining balance.</param>
/// <param name="Limit">The effective limit for the current period (-1 = unlimited).</param>
/// <param name="Consumed">Amount already consumed in the current period.</param>
/// <param name="Remaining">Remaining balance (<see cref="long.MaxValue"/> when unlimited).</param>
public sealed record QuotaCheckResult(bool Allowed, long Limit, long Consumed, long Remaining);
