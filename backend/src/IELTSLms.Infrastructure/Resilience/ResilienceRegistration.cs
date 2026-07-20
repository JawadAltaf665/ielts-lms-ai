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

        services.AddResiliencePipeline<string, HttpResponseMessage>("http-default", builder =>
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
        });

        return services;
    }
}
