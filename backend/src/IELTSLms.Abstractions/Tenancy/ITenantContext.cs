namespace IELTSLms.Abstractions.Tenancy;

/// <summary>
/// The ambient, resolved tenant for the current request/scope — the read side of the tenant context
/// (Multi-Tenant Architecture §5.2, §7). It is established once at the edge by tenant resolution and
/// then treated as immutable for the lifetime of the scope; downstream layers consume it and never
/// re-derive the tenant. The persistence layer reads it to apply the global tenant query filter.
/// </summary>
public interface ITenantContext
{
    /// <summary>The resolved tenant id, or <see langword="null"/> when no tenant has been established.</summary>
    Guid? TenantId { get; }

    /// <summary><see langword="true"/> when a tenant has been resolved for this scope.</summary>
    bool HasTenant { get; }

    /// <summary>
    /// <see langword="true"/> when the tenant query filter is intentionally bypassed for this scope
    /// (system administration, background jobs, cross-tenant reporting). Bypass is explicit and
    /// audited; it is never the default (Multi-Tenant Architecture §6.2).
    /// </summary>
    bool IsBypassed { get; }
}
