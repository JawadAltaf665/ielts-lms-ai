using System.Security.Claims;
using IELTSLms.Abstractions.Context;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Context;

/// <summary>
/// Resolves <see cref="ICurrentContext"/> from the ASP.NET Core request. This is the composition
/// root's adapter for the ambient-context port; claim parsing is intentionally minimal until
/// authentication and tenant resolution are implemented in a later sprint.
/// </summary>
internal sealed class HttpCurrentContext(IHttpContextAccessor httpContextAccessor) : ICurrentContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public Guid? TenantId =>
        Guid.TryParse(User?.FindFirstValue("tenantId"), out var tenantId) ? tenantId : null;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
