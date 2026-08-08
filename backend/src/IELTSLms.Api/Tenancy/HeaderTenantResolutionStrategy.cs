namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the <c>X-Tenant-Id</c> request header (Multi-Tenant Architecture §5.1).
/// Documented as "trusted server-to-server / admin tooling only" — for genuine browser traffic the
/// host is the pre-auth signal (<see cref="HostTenantResolutionStrategy"/>); this header is a
/// fallback for callers that cannot rely on their Host header carrying the tenant (server-to-server
/// callers, admin tooling, and — pragmatically — local development against a bare, non-subdomained
/// host such as <c>localhost</c>, where no tenant can be inferred from the host at all). It is
/// ordered <em>after</em> both the JWT claim and host strategies: for an authenticated request the
/// token wins, and the middleware treats a header that contradicts either as a spoofing attempt
/// rather than honouring it.
/// </summary>
public sealed class HeaderTenantResolutionStrategy : ITenantResolutionStrategy
{
    /// <summary>The request header carrying the tenant id for pre-auth requests.</summary>
    public const string HeaderName = "X-Tenant-Id";

    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public string Name => "header";

    /// <inheritdoc />
    public Task<Guid?> Resolve(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return Task.FromResult<Guid?>(null);
        }

        var raw = values.ToString();
        return Task.FromResult(Guid.TryParse(raw, out var tenantId) ? (Guid?)tenantId : null);
    }
}
