using System.Security.Claims;
using IELTSLms.Abstractions.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Infrastructure.Context;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsSystem => false;

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
}
