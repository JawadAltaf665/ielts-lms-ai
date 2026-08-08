using System.Security.Claims;

namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the authenticated principal's <c>tenantId</c> claim. This is the
/// authoritative source once a request is authenticated (Multi-Tenant Architecture §5.1): the claim
/// is minted by us at login and signed, so it cannot be forged by the caller. It is ordered ahead of
/// the header strategy so a supplied header can never override the token's tenant.
/// </summary>
public sealed class JwtClaimTenantResolutionStrategy : ITenantResolutionStrategy
{
    /// <summary>The access-token claim carrying the tenant id (Auth §9.1).</summary>
    public const string TenantClaimType = "tenantId";

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public string Name => "jwt-claim";

    /// <inheritdoc />
    public Task<Guid?> Resolve(HttpContext httpContext)
    {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<Guid?>(null);
        }

        var raw = user.FindFirstValue(TenantClaimType);
        return Task.FromResult(Guid.TryParse(raw, out var tenantId) ? (Guid?)tenantId : null);
    }
}
