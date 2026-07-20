using Hangfire;
using Hangfire.PostgreSql;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.BackgroundJobs.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.BackgroundJobs;

/// <summary>
/// Composition helpers for background processing. Configures Hangfire with PostgreSQL storage,
/// queue strategy, retry policies, and registers application-facing job service abstractions
/// (Backend Architecture §5.9).
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the Hangfire storage connection string.</summary>
    public const string ConnectionStringName = "Hangfire";

    public static class Queues
    {
        public const string Default = "default";
        public const string Critical = "critical";
        public const string Low = "low";
    }

    /// <summary>Registers Hangfire client, storage, server, and job service abstractions.</summary>
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' (or 'Database') was not found in configuration.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = System.Environment.ProcessorCount * 2;
            options.Queues = [Queues.Critical, Queues.Default, Queues.Low];
            options.StopTimeout = TimeSpan.FromSeconds(30);
            options.ShutdownTimeout = TimeSpan.FromSeconds(15);
        });

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = 3,
            DelaysInSeconds = [60, 300, 900],
            OnAttemptsExceeded = AttemptsExceededAction.Fail,
        });

        services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();
        services.AddSingleton<IRecurringJobService, HangfireRecurringJobService>();

        return services;
    }
}
