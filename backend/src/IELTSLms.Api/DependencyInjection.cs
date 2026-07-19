using Asp.Versioning;
using IELTSLms.Abstractions.Context;
using IELTSLms.Api.Authentication;
using IELTSLms.Api.Context;
using IELTSLms.Api.Middleware;
using IELTSLms.Api.Tenancy;
using Microsoft.OpenApi;
using PersistenceModule = IELTSLms.Persistence.DependencyInjection;
using InfrastructureModule = IELTSLms.Infrastructure.DependencyInjection;

namespace IELTSLms.Api;

/// <summary>
/// Composition helpers for presentation-layer concerns: Problem Details, current-context adapter,
/// API versioning, Swagger/OpenAPI, and health checks.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>Health-check tag identifying readiness probes (dependency checks).</summary>
    public const string ReadinessTag = "ready";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentContext, HttpCurrentContext>();

        // Tenant-resolution strategy chain (ordered by priority in the middleware): the authenticated
        // JWT claim is authoritative, the X-Tenant-Id header is an advisory pre-auth fallback.
        services.AddSingleton<ITenantResolutionStrategy, JwtClaimTenantResolutionStrategy>();
        services.AddSingleton<ITenantResolutionStrategy, HeaderTenantResolutionStrategy>();

        // RFC 7807 Problem Details for framework-generated errors (404, 415, ...); the global
        // handler and ResultExtensions cover exceptions and domain failures respectively.
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddJwtAuthentication(configuration);
        services.AddApiVersioningSupport();
        services.AddSwagger();
        services.AddHealthChecksSupport(configuration);

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

            // JWT bearer so protected endpoints can be exercised from Swagger UI.
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
    }
}
