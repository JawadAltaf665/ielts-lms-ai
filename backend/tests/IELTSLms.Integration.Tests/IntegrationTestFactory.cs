using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IELTSLms.Integration.Tests;

/// <summary>
/// Boots the API host for tests without external infrastructure. The Hangfire background server
/// polls PostgreSQL on start, which would hang the host in CI, so its hosted service is removed;
/// these smoke tests only exercise the HTTP surface and composition root, not job processing.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            // Deterministic config: give Hangfire a connection string so storage initialises,
            // and leave Redis empty so its readiness check is not registered.
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=ielts_lms_test;Username=postgres;Password=postgres",
                ["ConnectionStrings:Hangfire"] = "Host=localhost;Database=ielts_lms_test;Username=postgres;Password=postgres",
                ["ConnectionStrings:Redis"] = string.Empty,
            });
        });

        builder.ConfigureServices(services =>
        {
            // Drop hosted services (notably the Hangfire server) so the host doesn't poll external
            // infrastructure; these smoke tests exercise only the HTTP surface.
            services.RemoveAll<IHostedService>();
        });
    }
}
