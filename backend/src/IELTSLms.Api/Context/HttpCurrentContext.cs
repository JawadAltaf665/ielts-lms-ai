using System.Security.Claims;
using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Tenancy;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Context;

/// <summary>
/// Resolves <see cref="ICurrentContext"/> from the ASP.NET Core request. This is the composition
/// root's adapter for the ambient-context port. The tenant is taken from the resolved
/// <see cref="ITenantContext"/> — established once by <c>TenantResolutionMiddleware</c> — rather than
/// re-parsed from the claim here, so the whole request shares one authoritative tenant value.
/// </summary>
internal sealed class HttpCurrentContext(
    IHttpContextAccessor httpContextAccessor,
    ITenantContext tenantContext) : ICurrentContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public Guid? TenantId => tenantContext.TenantId;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
