namespace IELTSLms.Api.Tenancy;

/// <summary>
/// One way of discovering which tenant a request belongs to (Multi-Tenant Architecture §5.1).
/// Strategies are tried in priority order by <see cref="TenantResolutionMiddleware"/>; the first to
/// return a non-null tenant wins. New strategies (subdomain, custom domain, path segment) can be
/// added by implementing this interface and registering them — the middleware needs no changes.
/// </summary>
public interface ITenantResolutionStrategy
{
    /// <summary>
    /// Priority order, lowest first. The authoritative source (the authenticated token) must sort
    /// before advisory sources (host, header) so it cannot be overridden (§5.1).
    /// </summary>
    int Order { get; }

    /// <summary>A short name for diagnostics and logging (e.g. "jwt-claim", "host", "header").</summary>
    string Name { get; }

    /// <summary>
    /// Attempts to resolve the tenant from the request. Returns the tenant id, or <see langword="null"/>
    /// if this strategy does not apply. Must not throw for a merely-absent signal. Asynchronous because
    /// host-based resolution looks the slug up in the database.
    /// </summary>
    Task<Guid?> Resolve(HttpContext httpContext);
}
