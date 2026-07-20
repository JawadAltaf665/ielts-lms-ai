using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Persistence.Seeding;

/// <summary>
/// Ensures the well-known system tenant exists in the database on startup. This row is required
/// as the FK target for all identity entities seeded during Sprint 2 (which used the placeholder
/// <c>00000000-0000-0000-0000-000000000001</c>). Idempotent — runs once per cold start. If the
/// database is unreachable the seeder logs a warning and allows the host to continue; the seed
/// will succeed on the next restart once the database is available.
/// </summary>
internal sealed partial class SystemTenantSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemTenantSeeder> logger) : IHostedService
{
    /// <summary>The well-known system tenant id matching <c>IdentityDefaults.TenantId</c>.</summary>
    public static readonly Guid SystemTenantId = new("00000000-0000-0000-0000-000000000001");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var exists = await dbContext.Set<Tenant>()
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == TenantId.From(SystemTenantId), cancellationToken)
                .ConfigureAwait(false);

            if (exists)
            {
                SystemTenantAlreadyExists(logger);
                return;
            }

            var tenant = Tenant.Create(
                TenantId.From(SystemTenantId),
                TenantSlug.Create("system").Value,
                TenantCode.Create("SYSTEM").Value,
                "System",
                DateTimeOffset.UtcNow,
                primaryDomain: null,
                contactEmail: null,
                defaultLocale: Tenant.FallbackLocale,
                defaultTimeZone: Tenant.FallbackTimeZone);

            dbContext.Set<Tenant>().Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            SystemTenantSeeded(logger);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SeedingFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(EventId = 4000, Level = LogLevel.Information, Message = "System tenant already exists; skipping seed.")]
    private static partial void SystemTenantAlreadyExists(ILogger logger);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "Seeded system tenant (00000000-0000-0000-0000-000000000001).")]
    private static partial void SystemTenantSeeded(ILogger logger);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Warning, Message = "Failed to seed system tenant; the database may be unavailable. Seeding will retry on next startup.")]
    private static partial void SeedingFailed(ILogger logger, Exception exception);
}
