namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the <c>X-Tenant-Id</c> request header (Multi-Tenant Architecture §5.1).
/// This is an advisory source used before a request is authenticated — for example so a login or
/// registration request can be attributed to the right tenant. It is ordered <em>after</em> the JWT
/// claim strategy: for an authenticated request the token wins, and the middleware treats a header
/// that contradicts the token as a spoofing attempt rather than honouring it.
/// </summary>
public sealed class HeaderTenantResolutionStrategy : ITenantResolutionStrategy
{
    /// <summary>The request header carrying the tenant id for pre-auth requests.</summary>
    public const string HeaderName = "X-Tenant-Id";

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public string Name => "header";

    /// <inheritdoc />
    public Guid? Resolve(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return null;
        }

        var raw = values.ToString();
        return Guid.TryParse(raw, out var tenantId) ? tenantId : null;
    }
}
