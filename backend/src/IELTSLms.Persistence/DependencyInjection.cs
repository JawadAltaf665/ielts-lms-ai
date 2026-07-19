using IELTSLms.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Persistence;

/// <summary>
/// Composition helpers for the persistence layer: registers the EF Core context against PostgreSQL
/// and exposes it as the application's <see cref="IUnitOfWork"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the primary PostgreSQL connection string.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>Registers persistence-layer services into the container.</summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found in configuration.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure();
            });
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
