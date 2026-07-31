using Hangfire;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Api;
using IELTSLms.Api.Endpoints;
using IELTSLms.Api.HealthChecks;
using IELTSLms.Api.Middleware;
using IELTSLms.Api.Tenancy;
using IELTSLms.Application;
using IELTSLms.Application.Vocabulary;
using IELTSLms.BackgroundJobs;
using IELTSLms.BackgroundJobs.Billing;
using IELTSLms.Infrastructure;
using IELTSLms.Infrastructure.Performance;
using IELTSLms.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using System.Globalization;
using System.Threading.RateLimiting;
using ApiDi = IELTSLms.Api.DependencyInjection;

// Bootstrap logger: active before the host is built so startup failures are captured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Kestrel tuning (Performance §4)
    var perfOptions = builder.Configuration
        .GetSection(PerformanceOptions.SectionName)
        .Get<PerformanceOptions>() ?? new PerformanceOptions();

    builder.WebHost.ConfigureKestrel(kestrel =>
    {
        kestrel.Limits.MaxRequestBodySize = perfOptions.MaxRequestBodySizeBytes;
        kestrel.Limits.MaxConcurrentConnections = perfOptions.MaxConcurrentConnections;
        kestrel.Limits.MaxConcurrentUpgradedConnections = perfOptions.MaxConcurrentUpgradedConnections;
        kestrel.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(perfOptions.KeepAliveTimeoutSeconds);
        kestrel.Limits.MaxRequestHeaderCount = perfOptions.MaxRequestHeaderCount;
        kestrel.Limits.MaxRequestHeadersTotalSize = perfOptions.MaxRequestHeadersTotalSizeBytes;
    });

    // Replace the bootstrap logger with the fully configured one, read from configuration.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("ApplicationName", "ielts-lms-api")
        .Destructure.With(new IELTSLms.Infrastructure.Logging.SensitiveDataMaskingPolicy()));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddPersistence(builder.Configuration)
        .AddBackgroundJobs(builder.Configuration)
        .AddPresentation(builder.Configuration, builder.Environment);

    // VocabularyOptions lives in Application; bind it here where configuration is available
    // (mirrors AuthOptions binding in AuthenticationRegistration).
    builder.Services.AddOptions<VocabularyOptions>()
        .Bind(builder.Configuration.GetSection(VocabularyOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var app = builder.Build();

    // --- Middleware pipeline (Backend Architecture §7) ---

    // Exception handler must be first to catch everything downstream.
    app.UseExceptionHandler();

    // Response compression (before anything writes the body).
    if (perfOptions.EnableResponseCompression)
    {
        app.UseResponseCompression();
    }

    // Forwarded headers (must be early for correct remote IP in rate limiting/logging).
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
            | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
    });

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestContextMiddleware>();
    app.UseMiddleware<PerformanceMiddleware>();

    // RFC 7807 problem responses for framework-generated status codes (404, 405).
    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // Security response headers (Security Standards §12).
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // CORS (must be before auth/endpoints).
    app.UseCors(ApiProtectionRegistration.CorsPolicyName);

    // Rate limiting (after CORS, before auth).
    app.UseRateLimiter();

    // Request timeouts.
    app.UseRequestTimeouts();

    // Authentication / tenant / authorization.
    app.UseAuthentication();
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseAuthorization();

    // API versioning + routes.
    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
        .ReportApiVersions()
        .Build();

    var api = app
        .MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);
    api.MapVersionEndpoints();
    api.MapAuthEndpoints();
    api.MapTenantEndpoints();
    api.MapBillingEndpoints();
    api.MapGrammarEndpoints();
    api.MapVocabularyEndpoints();
    api.MapAiEndpoints();
    api.MapReadingEndpoints();
    api.MapListeningEndpoints();
    api.MapFilesEndpoints();
    api.MapWebhookEndpoints();

    // Health endpoints.
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApiDi.ReadinessTag),
    });

    app.MapHealthChecks("/health/startup", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApiDi.StartupTag),
    });

    app.MapHealthChecks("/health/detail", HealthCheckResponseWriter.DetailedOptions);

    // Hangfire dashboard (development only; production uses a separate admin service).
    if (app.Environment.IsDevelopment())
    {
        app.MapHangfireDashboard("/hangfire");
    }

    // Billing recurring jobs (Phase 5 Sprint 1 — 13 §12). Skipped under "Testing": AddOrUpdate writes
    // to Hangfire's PostgreSQL storage eagerly (unlike the Hangfire server, which IntegrationTestFactory
    // already strips via RemoveAll<IHostedService>()), so registering here would make every
    // WebApplicationFactory-based test require a real Postgres instance just to boot the host.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var recurringJobs = app.Services.GetRequiredService<IRecurringJobService>();
        recurringJobs.AddOrUpdate<TrialExpirationSweepJob>(
            "billing-trial-expiration-sweep", job => job.ExecuteAsync(CancellationToken.None), "0 * * * *");
        recurringJobs.AddOrUpdate<SubscriptionExpirationSweepJob>(
            "billing-subscription-expiration-sweep", job => job.ExecuteAsync(CancellationToken.None), "15 * * * *");
        recurringJobs.AddOrUpdate<PastDueSuspensionSweepJob>(
            "billing-past-due-suspension-sweep", job => job.ExecuteAsync(CancellationToken.None), "0 3 * * *");
        recurringJobs.AddOrUpdate<RenewalReminderJob>(
            "billing-renewal-reminder", job => job.ExecuteAsync(CancellationToken.None), "0 9 * * *");
        recurringJobs.AddOrUpdate<QuotaResetSweepJob>(
            "billing-quota-reset-sweep", job => job.ExecuteAsync(CancellationToken.None), "0 0 * * *");
        recurringJobs.AddOrUpdate<StripeReconciliationJob>(
            "billing-stripe-reconciliation", job => job.ExecuteAsync(CancellationToken.None), "0 */6 * * *");
        recurringJobs.AddOrUpdate<DeadLetterWebhookRetryJob>(
            "billing-dead-letter-webhook-retry", job => job.ExecuteAsync(CancellationToken.None), "*/15 * * * *");
    }

    // Graceful shutdown: wait for in-flight requests to complete.
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("Application is shutting down gracefully...");
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
