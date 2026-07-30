using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace IELTSLms.Infrastructure.Resilience;

/// <summary>
/// Registers default resilience pipelines using Microsoft.Extensions.Resilience / Polly v8.
/// Provides named pipelines that consumers can reference for HTTP clients or manual use.
/// Covers: retry with exponential backoff + jitter, circuit breaker, and timeout
/// (Infrastructure §3, Development Standards §18).
/// </summary>
public static class ResilienceRegistration
{
    public const string DefaultPipelineName = "default";

    /// <summary>Named HTTP-response pipeline consumers attach to their <see cref="HttpClient"/> registrations
    /// via <c>ResiliencePipelineProvider&lt;string&gt;.GetPipeline&lt;HttpResponseMessage&gt;</c>.</summary>
    public const string DefaultHttpPipelineName = "http-default";

    public static IServiceCollection AddResiliencePolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ResilienceOptions.SectionName)
            .Get<ResilienceOptions>() ?? new ResilienceOptions();

        services.AddResiliencePipeline(DefaultPipelineName, builder =>
        {
            builder
                .AddRetry(new()
                {
                    MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                    Delay = TimeSpan.FromMilliseconds(options.Retry.BaseDelayMs),
                    MaxDelay = TimeSpan.FromMilliseconds(options.Retry.MaxDelayMs),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = options.Retry.UseJitter,
                })
                .AddCircuitBreaker(new()
                {
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.BreakDurationSeconds),
                    FailureRatio = options.CircuitBreaker.FailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
                    MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                })
                .AddTimeout(TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds));
        });

        services.AddResiliencePipeline<string, HttpResponseMessage>(
            DefaultHttpPipelineName, builder => ConfigureHttpPipeline(builder, options));

        return services;
    }

    /// <summary>
    /// Applies the standard retry/circuit-breaker/timeout shape to an HTTP-response pipeline builder.
    /// Shared by the named <see cref="DefaultHttpPipelineName"/> pipeline above and by individual
    /// <c>HttpClient</c> registrations (e.g. <c>AddAi</c>) that want the same policy without a second
    /// hop through the keyed pipeline provider.
    /// </summary>
    public static void ConfigureHttpPipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder, ResilienceOptions options)
    {
        builder
            .AddRetry(new()
            {
                MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(options.Retry.BaseDelayMs),
                MaxDelay = TimeSpan.FromMilliseconds(options.Retry.MaxDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = options.Retry.UseJitter,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                        || r.StatusCode >= System.Net.HttpStatusCode.InternalServerError),
            })
            .AddCircuitBreaker(new()
            {
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.BreakDurationSeconds),
                FailureRatio = options.CircuitBreaker.FailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
                MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
            })
            .AddTimeout(TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds));
    }

    /// <summary>Reads the <c>Resilience</c> section, falling back to defaults if unset.</summary>
    public static ResilienceOptions ReadOptions(IConfiguration configuration) =>
        configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();
}
