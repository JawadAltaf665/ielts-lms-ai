using IELTSLms.Abstractions.Caching;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Time;
using IELTSLms.Infrastructure.Caching;
using IELTSLms.Infrastructure.Events;
using IELTSLms.Infrastructure.Observability;
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

        AddCaching(services, configuration);
        services.AddObservability(configuration);

        return services;
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
