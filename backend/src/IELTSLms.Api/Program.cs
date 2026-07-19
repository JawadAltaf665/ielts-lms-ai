using IELTSLms.Api;
using IELTSLms.Api.Endpoints;
using IELTSLms.Api.Middleware;
using IELTSLms.Application;
using IELTSLms.BackgroundJobs;
using IELTSLms.Infrastructure;
using IELTSLms.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Globalization;

// Bootstrap logger: active before the host is built so startup failures are captured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace the bootstrap logger with the fully configured one, read from configuration.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddPersistence(builder.Configuration)
        .AddBackgroundJobs(builder.Configuration)
        .AddPresentation(builder.Configuration);

    var app = builder.Build();

    // --- Middleware pipeline (Backend Architecture §7): exception → correlation → ... → routing ---
    app.UseExceptionHandler();

    app.UseMiddleware<CorrelationIdMiddleware>();

    // Emit RFC 7807 problem responses for framework-generated status codes (e.g. 404 with no
    // matching endpoint, 405) that never reach the exception handler.
    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Authentication and tenant resolution middleware are added in a later sprint.

    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
        .ReportApiVersions()
        .Build();

    var api = app
        .MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);
    api.MapVersionEndpoints();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        // Liveness: process is up. Exclude dependency checks so a dependency blip doesn't recycle us.
        Predicate = _ => false,
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        // Readiness: dependencies (DB, cache) are reachable.
        Predicate = check => check.Tags.Contains(IELTSLms.Api.DependencyInjection.ReadinessTag),
    });

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Exposed so the integration-test host (WebApplicationFactory) can reference the entry point.</summary>
public partial class Program;
