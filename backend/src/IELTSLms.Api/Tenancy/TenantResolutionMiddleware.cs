using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Domain.Tenancy;

namespace IELTSLms.Api.Tenancy;

/// <summary>
/// Establishes the ambient tenant for the request exactly once, at the edge, from the registered
/// <see cref="ITenantResolutionStrategy"/> chain (Multi-Tenant Architecture §5). Strategies are tried
/// in priority order and the first non-null result is authoritative. To defeat tenant spoofing
/// (§5.1), any lower-priority signal that contradicts the authoritative one — most importantly an
/// <c>X-Tenant-Id</c> header that disagrees with the authenticated token — fails the request rather
/// than being silently honoured or ignored.
///
/// After resolution, the middleware validates that the tenant exists and is in an active state. If
/// validation fails, the request is rejected with an RFC 7807 problem response. Paths that operate
/// without a tenant (health, version, tenant management endpoints) are skipped.
/// </summary>
/// <remarks>
/// Must run after authentication so the JWT claim strategy sees a populated principal. Requests that
/// resolve no tenant are allowed through with an unset context; endpoints that require a tenant fail
/// closed downstream, and system endpoints (health, docs) that legitimately have no tenant are
/// skipped here.
/// </remarks>
public sealed class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    private static readonly HashSet<string> BypassPrefixes =
    [
        "/health",
        "/version",
        "/swagger",
    ];

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextAccessor tenantContext,
        ITenantRepository tenantRepository,
        IEnumerable<ITenantResolutionStrategy> strategies)
    {
        var ordered = strategies.OrderBy(s => s.Order).ToArray();

        Guid? resolved = null;
        string? resolvedBy = null;

        foreach (var strategy in ordered)
        {
            var candidate = strategy.Resolve(context);
            if (candidate is not { } tenantId)
            {
                continue;
            }

            if (resolved is null)
            {
                resolved = tenantId;
                resolvedBy = strategy.Name;
                continue;
            }

            if (tenantId != resolved.Value)
            {
                TenantConflictDetected(logger, resolvedBy!, strategy.Name);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Conflicting tenant context.").ConfigureAwait(false);
                return;
            }
        }

        if (resolved is { } finalTenant)
        {
            if (!ShouldBypassValidation(context))
            {
                var tenant = await tenantRepository.GetByIdAsync(
                    TenantId.From(finalTenant), context.RequestAborted).ConfigureAwait(false);

                if (tenant is null)
                {
                    TenantNotFound(logger, finalTenant);
                    await WriteProblem(context, StatusCodes.Status404NotFound,
                        "TENANT_NOT_FOUND", "The specified tenant does not exist.").ConfigureAwait(false);
                    return;
                }

                if (tenant.Status != TenantStatus.Active)
                {
                    TenantInactive(logger, finalTenant, tenant.Status);
                    await WriteProblem(context, StatusCodes.Status403Forbidden,
                        "TENANT_INACTIVE", $"The tenant is {tenant.Status.ToString().ToLowerInvariant()} and cannot serve requests.").ConfigureAwait(false);
                    return;
                }
            }

            tenantContext.SetTenant(finalTenant);
            TenantResolved(logger, finalTenant, resolvedBy!);
        }

        await next(context).ConfigureAwait(false);
    }

    private static bool ShouldBypassValidation(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        foreach (var prefix in BypassPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Tenant management endpoints operate cross-tenant (admin provisioning).
        if (path.Contains("/tenants", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static async Task WriteProblem(HttpContext context, int statusCode, string code, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = statusCode == StatusCodes.Status404NotFound ? "Not Found" : "Forbidden",
            status = statusCode,
            detail,
            code,
            instance = context.Request.Path.Value,
        }).ConfigureAwait(false);
    }

    private static readonly Action<ILogger, Guid, string, Exception?> TenantResolvedMessage =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Debug,
            new EventId(3000, nameof(TenantResolved)),
            "Resolved tenant {TenantId} via {Strategy}.");

    private static readonly Action<ILogger, string, string, Exception?> TenantConflictMessage =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(3001, nameof(TenantConflictDetected)),
            "Tenant conflict: {Authoritative} resolved a tenant that {Conflicting} contradicts; rejecting request.");

    private static readonly Action<ILogger, Guid, Exception?> TenantNotFoundMessage =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(3002, nameof(TenantNotFound)),
            "Resolved tenant {TenantId} does not exist in the database; rejecting request.");

    private static readonly Action<ILogger, Guid, TenantStatus, Exception?> TenantInactiveMessage =
        LoggerMessage.Define<Guid, TenantStatus>(
            LogLevel.Warning,
            new EventId(3003, nameof(TenantInactive)),
            "Resolved tenant {TenantId} has status {Status}; rejecting request.");

    private static void TenantResolved(ILogger logger, Guid tenantId, string strategy) =>
        TenantResolvedMessage(logger, tenantId, strategy, null);

    private static void TenantConflictDetected(ILogger logger, string authoritative, string conflicting) =>
        TenantConflictMessage(logger, authoritative, conflicting, null);

    private static void TenantNotFound(ILogger logger, Guid tenantId) =>
        TenantNotFoundMessage(logger, tenantId, null);

    private static void TenantInactive(ILogger logger, Guid tenantId, TenantStatus status) =>
        TenantInactiveMessage(logger, tenantId, status, null);
}
