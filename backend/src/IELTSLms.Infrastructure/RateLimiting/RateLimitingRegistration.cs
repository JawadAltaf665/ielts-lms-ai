using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IELTSLms.Infrastructure.RateLimiting;

/// <summary>
/// Configures ASP.NET Core rate limiting with global, per-IP, per-user, and per-tenant policies.
/// Uses sliding-window for per-IP (smoother burst distribution) and fixed-window for global/user/tenant.
/// All thresholds are configuration-driven (API Design Standards §8, Security §13).
/// </summary>
public static class RateLimitingRegistration
{
    public static class PolicyNames
    {
        public const string Global = "global";
        public const string PerIp = "per-ip";
        public const string PerUser = "per-user";
        public const string PerTenant = "per-tenant";
    }

    public static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                ctx.HttpContext.Response.ContentType = "application/problem+json";
                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Try again later.",
                    code = "RATE_LIMIT_EXCEEDED",
                }, ct);
            };

            // Global fixed-window limiter.
            limiter.AddPolicy(PolicyNames.Global, _ =>
                RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.Global.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.Global.WindowSeconds),
                    QueueLimit = options.Global.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                }));

            // Per-IP sliding-window limiter.
            limiter.AddPolicy(PolicyNames.PerIp, ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                if (environment.IsDevelopment() && ctx.Request.Path.StartsWithSegments("/swagger"))
                {
                    return RateLimitPartition.GetNoLimiter(ip);
                }

                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.PerIp.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.PerIp.WindowSeconds),
                    SegmentsPerWindow = options.PerIp.SegmentsPerWindow,
                    QueueLimit = options.PerIp.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                });
            });

            // Per-user fixed-window limiter (authenticated requests only).
            limiter.AddPolicy(PolicyNames.PerUser, ctx =>
            {
                var userId = ctx.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                if (userId == "anonymous")
                {
                    return RateLimitPartition.GetNoLimiter(userId);
                }

                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PerUser.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.PerUser.WindowSeconds),
                    QueueLimit = options.PerUser.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                });
            });

            // Per-tenant fixed-window limiter.
            limiter.AddPolicy(PolicyNames.PerTenant, ctx =>
            {
                var tenantId = ctx.User?.FindFirstValue("tenant_id") ?? "global";
                if (tenantId == "global")
                {
                    return RateLimitPartition.GetNoLimiter(tenantId);
                }

                return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PerTenant.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.PerTenant.WindowSeconds),
                    QueueLimit = options.PerTenant.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                });
            });
        });

        return services;
    }
}
