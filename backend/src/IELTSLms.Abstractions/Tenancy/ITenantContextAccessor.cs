namespace IELTSLms.Abstractions.Tenancy;

/// <summary>
/// The write side of the ambient tenant context: lets the edge (tenant-resolution middleware) and
/// trusted infrastructure (job hosts, seeders) establish or bypass the tenant for a scope. Kept
/// separate from <see cref="ITenantContext"/> so ordinary application code can read the tenant but
/// cannot silently reassign it (Multi-Tenant Architecture §5.2, §7).
/// </summary>
public interface ITenantContextAccessor : ITenantContext
{
    /// <summary>
    /// Binds the resolved tenant to the current scope. Intended to be called exactly once per request
    /// by tenant-resolution middleware; re-binding a different tenant is a programming error.
    /// </summary>
    void SetTenant(Guid tenantId);

    /// <summary>
    /// Enters an explicit, disposable filter-bypass scope for privileged, cross-tenant work
    /// (Multi-Tenant Architecture §6.2). Disposing restores the prior state, so bypass never leaks
    /// past the intended operation.
    /// </summary>
    IDisposable BypassTenantFilter();
}
