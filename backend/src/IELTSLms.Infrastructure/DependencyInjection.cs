using IELTSLms.Abstractions.Caching;
using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Infrastructure.Caching;
using IELTSLms.Infrastructure.Email;
using IELTSLms.Infrastructure.Events;
using IELTSLms.Infrastructure.Observability;
using IELTSLms.Infrastructure.Security;
using IELTSLms.Infrastructure.Tenancy;
using IELTSLms.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Infrastructure;

/// <summary>
/// Composition helpers for the infrastructure layer: technology adapters (clock, ids, cache,
/// domain-event dispatch) and cross-cutting observability.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the Redis cache connection string.</summary>
    public const string RedisConnectionStringName = "Redis";

    /// <summary>Registers infrastructure-layer services into the container.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGuidProvider, GuidProvider>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // One tenant-context instance per request scope; the accessor (write side) and the read-only
        // view resolve to the same object so resolution middleware and consumers share state.
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContextAccessor>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        AddSecurity(services, configuration);
        AddEmail(services);
        AddCaching(services, configuration);
        services.AddObservability(configuration);

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

        // One shared key for signing (here) and validation (API bearer middleware).
        services.AddSingleton<JwtSigningKeyProvider>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IPasswordPolicy, ConfigurablePasswordPolicy>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
    }

    private static void AddEmail(IServiceCollection services)
    {
        // Transport is interface-only this sprint: a logging sender stands in for SMTP/SendGrid.
        services.AddSingleton<IEmailTemplateRenderer, IdentityEmailTemplateRenderer>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString(RedisConnectionStringName);

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // No Redis configured (local dev / tests): fall back to an in-process distributed cache
            // so ICacheService still resolves and behaves identically from the caller's view.
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ielts-lms:";
            });
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
    }
}
