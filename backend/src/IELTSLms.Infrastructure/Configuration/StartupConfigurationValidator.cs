using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Infrastructure.Configuration;

/// <summary>
/// Startup validation that fails fast when critical configuration is missing or invalid.
/// Runs as an <see cref="IHostedService"/> so the application doesn't start serving
/// requests with broken configuration (Development Standards §15).
/// </summary>
public sealed partial class StartupConfigurationValidator : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<StartupConfigurationValidator> _logger;

    public StartupConfigurationValidator(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<StartupConfigurationValidator> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        ValidateConnectionStrings(errors);
        ValidateJwt(errors);
        ValidateLogging(errors);
        ValidateObservability(errors);

        if (errors.Count > 0)
        {
            var message = string.Join("; ", errors);

            LogValidationFailed(_logger, errors.Count, message);

            if (!_environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    $"Startup configuration validation failed with {errors.Count} error(s): {message}");
            }

            LogContinuingDespiteErrors(_logger);
        }
        else
        {
            LogValidationPassed(_logger);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void ValidateConnectionStrings(List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(_configuration.GetConnectionString("Database")))
        {
            errors.Add("ConnectionStrings:Database is required.");
        }

        if (string.IsNullOrWhiteSpace(_configuration.GetConnectionString("Redis")))
        {
            LogRedisNotConfigured(_logger);
        }

        if (string.IsNullOrWhiteSpace(_configuration.GetConnectionString("Hangfire"))
            && string.IsNullOrWhiteSpace(_configuration.GetConnectionString("Database")))
        {
            errors.Add("ConnectionStrings:Hangfire (or Database fallback) is required.");
        }
    }

    private void ValidateJwt(List<string> errors)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            errors.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            errors.Add("Jwt:Audience is required.");
        }

        if (!_environment.IsDevelopment())
        {
            if (string.IsNullOrWhiteSpace(_configuration["Jwt:PrivateKeyPem"]))
            {
                errors.Add("Jwt:PrivateKeyPem is required in non-development environments.");
            }

            if (string.IsNullOrWhiteSpace(_configuration["Jwt:PublicKeyPem"]))
            {
                errors.Add("Jwt:PublicKeyPem is required in non-development environments.");
            }
        }
    }

    private void ValidateLogging(List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(_configuration["Serilog:MinimumLevel:Default"]))
        {
            errors.Add("Serilog:MinimumLevel:Default is required.");
        }
    }

    private void ValidateObservability(List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(_configuration["Observability:ServiceName"]))
        {
            errors.Add("Observability:ServiceName is required.");
        }
    }

    [LoggerMessage(Level = LogLevel.Critical, Message = "Startup validation failed with {ErrorCount} error(s): {Errors}")]
    private static partial void LogValidationFailed(ILogger logger, int errorCount, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Continuing despite validation errors (development mode).")]
    private static partial void LogContinuingDespiteErrors(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Startup configuration validation passed.")]
    private static partial void LogValidationPassed(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "ConnectionStrings:Redis is empty — using in-memory distributed cache.")]
    private static partial void LogRedisNotConfigured(ILogger logger);
}
