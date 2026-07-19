using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.BackgroundJobs;

/// <summary>
/// Composition helpers for background processing. Configures Hangfire with the PostgreSQL storage
/// backend and registers the server; concrete job definitions are added by feature slices later
/// (Backend Architecture §5.9).
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the Hangfire storage connection string.</summary>
    public const string ConnectionStringName = "Hangfire";

    /// <summary>Registers Hangfire client, storage, and server into the container.</summary>
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fall back to the primary database connection when a dedicated Hangfire one isn't set.
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
            options.WorkerCount = Environment.ProcessorCount * 2;
        });

        return services;
    }
}
