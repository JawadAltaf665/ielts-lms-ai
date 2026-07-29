using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Caching;
using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Environment;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Serialization;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Infrastructure.Billing;
using IELTSLms.Infrastructure.Caching;
using IELTSLms.Infrastructure.Configuration;
using IELTSLms.Infrastructure.Context;
using IELTSLms.Infrastructure.Email;
using IELTSLms.Infrastructure.Environment;
using IELTSLms.Infrastructure.Events;
using IELTSLms.Infrastructure.Observability;
using IELTSLms.Infrastructure.Performance;
using IELTSLms.Infrastructure.Resilience;
using IELTSLms.Infrastructure.Security;
using IELTSLms.Infrastructure.Serialization;
using IELTSLms.Infrastructure.Tenancy;
using IELTSLms.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IELTSLms.Infrastructure;

/// <summary>
/// Composition helpers for the infrastructure layer: technology adapters (clock, ids, cache,
/// domain-event dispatch), cross-cutting observability, resilience, performance, and CORS.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the Redis cache connection string.</summary>
    public const string RedisConnectionStringName = "Redis";

    /// <summary>Registers infrastructure-layer services into the container.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGuidProvider, GuidProvider>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // One tenant-context instance per request scope.
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContextAccessor>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // Context services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService>(sp =>
        {
            var tc = sp.GetRequiredService<ITenantContext>();
            return new CurrentTenantServiceAdapter(tc);
        });
        services.AddScoped<ICorrelationIdProvider, CorrelationIdAccessor>();
        services.AddScoped<IRequestContext, RequestContextAccessor>();

        // Utilities
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        services.AddSingleton<IEnvironmentService, EnvironmentService>();

        // Configuration
        services.AddValidatedOptions<LoggingOptions>(configuration, LoggingOptions.SectionName);
        services.AddValidatedOptions<AuditOptions>(configuration, AuditOptions.SectionName);
        services.AddValidatedOptions<CacheOptions>(configuration, CacheOptions.SectionName);
        services.AddValidatedOptions<PerformanceOptions>(configuration, PerformanceOptions.SectionName);

        AddSecurity(services, configuration);
        AddEmail(services);
        AddBilling(services, configuration);
        AddCaching(services, configuration);
        services.AddObservability(configuration);
        services.AddResiliencePolicies(configuration);
        services.AddPerformanceInfrastructure(configuration);
        services.AddApiProtection(configuration, environment);

        // Startup validation (hosted service — runs before first request)
        services.AddHostedService<StartupConfigurationValidator>();

        return services;
    }

    private static void AddSecurity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<Argon2Options>()
            .Bind(configuration.GetSection(Argon2Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PasswordPolicyOptions>()
            .Bind(configuration.GetSection(PasswordPolicyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JwtSigningKeyProvider>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IPasswordPolicy, ConfigurablePasswordPolicy>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
    }

    private static void AddEmail(IServiceCollection services)
    {
        services.AddSingleton<IEmailTemplateRenderer, IdentityEmailTemplateRenderer>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
    }

    private static void AddBilling(IServiceCollection services, IConfiguration configuration)
    {
        // Not ValidateOnStart (ADR-031 / StripeOptions doc comment): a blank Stripe key must not
        // prevent the whole API from starting in local dev or CI.
        services.AddOptions<StripeOptions>().Bind(configuration.GetSection(StripeOptions.SectionName));

        services.AddSingleton<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<IStripeWebhookVerifier, StripeWebhookVerifier>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString(RedisConnectionStringName);

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = configuration
                    .GetSection(CacheOptions.SectionName)
                    .Get<CacheOptions>()?.InstancePrefix ?? "ielts-lms:";
            });
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
    }
}
