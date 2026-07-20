using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IELTSLms.Infrastructure.Performance;

/// <summary>
/// Configures CORS, forwarded headers, HTTPS enforcement, and HSTS for the API.
/// In development all origins are allowed; in production only configured origins pass
/// (Security Standards §12.5).
/// </summary>
public static class ApiProtectionRegistration
{
    public const string CorsPolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddApiProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(cors =>
        {
            cors.AddPolicy(CorsPolicyName, policy =>
            {
                bool isWildcard = options.AllowedOrigins.Length == 0 || options.AllowedOrigins.Contains("*");
                bool canAllowCredentials = options.AllowCredentials;

                if (environment.IsDevelopment() && isWildcard)
                {
                    if (options.AllowCredentials)
                    {
                        policy.SetIsOriginAllowed(_ => true);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                        canAllowCredentials = false;
                    }
                }
                else if (options.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(options.AllowedOrigins);
                    if (options.AllowedOrigins.Contains("*"))
                    {
                        canAllowCredentials = false;
                    }
                }

                policy
                    .WithMethods(options.AllowedMethods)
                    .WithHeaders(options.AllowedHeaders)
                    .WithExposedHeaders(options.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(options.MaxAgeSeconds));

                if (canAllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        if (!environment.IsDevelopment())
        {
            services.AddHsts(hsts =>
            {
                hsts.MaxAge = TimeSpan.FromDays(365);
                hsts.IncludeSubDomains = true;
                hsts.Preload = true;
            });
        }

        return services;
    }
}
