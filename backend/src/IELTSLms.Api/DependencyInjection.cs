using Asp.Versioning;
using IELTSLms.Abstractions.Context;
using IELTSLms.Api.Authentication;
using IELTSLms.Api.Context;
using IELTSLms.Api.Middleware;
using IELTSLms.Api.Tenancy;
using IELTSLms.Infrastructure.HealthChecks;
using IELTSLms.Infrastructure.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using PersistenceModule = IELTSLms.Persistence.DependencyInjection;
using InfrastructureModule = IELTSLms.Infrastructure.DependencyInjection;

namespace IELTSLms.Api;

/// <summary>
/// Composition helpers for presentation-layer concerns: Problem Details, current-context adapter,
/// API versioning, Swagger/OpenAPI, health checks, and rate limiting.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>Health-check tag identifying readiness probes (dependency checks).</summary>
    public const string ReadinessTag = "ready";

    /// <summary>Health-check tag identifying startup probes.</summary>
    public const string StartupTag = "startup";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentContext, HttpCurrentContext>();

        // Tenant-resolution strategy chain.
        services.AddSingleton<ITenantResolutionStrategy, JwtClaimTenantResolutionStrategy>();
        services.AddSingleton<ITenantResolutionStrategy, HeaderTenantResolutionStrategy>();

        // RFC 7807 Problem Details for framework-generated errors.
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddJwtAuthentication(configuration);
        services.AddApiVersioningSupport();
        services.AddSwagger();
        services.AddHealthChecksSupport(configuration);
        services.AddRateLimitingPolicies(configuration, environment);

        return services;
    }

    private static void AddApiVersioningSupport(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "IELTS LMS AI API",
                Version = "v1",
                Description = "Backend foundation for IELTS LMS AI.",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT access token (without the 'Bearer' prefix).",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });
        });
    }

    private static void AddHealthChecksSupport(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        var databaseConnection = configuration.GetConnectionString(PersistenceModule.ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(databaseConnection))
        {
            healthChecks.AddNpgSql(
                databaseConnection,
                name: "postgres",
                tags: [ReadinessTag]);
        }

        var redisConnection = configuration.GetConnectionString(InfrastructureModule.RedisConnectionStringName);
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            healthChecks.AddRedis(
                redisConnection,
                name: "redis",
                tags: [ReadinessTag]);
        }

        // Custom health checks
        healthChecks
            .AddCheck<ApplicationHealthCheck>("application", tags: [StartupTag])
            .AddCheck<DiskSpaceHealthCheck>("disk-space", tags: [ReadinessTag])
            .AddCheck<MemoryHealthCheck>("memory", tags: [ReadinessTag])
            .AddCheck<ConfigurationHealthCheck>("configuration", tags: [StartupTag]);
    }
}
