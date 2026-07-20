using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Resilience;

/// <summary>
/// Binds the <c>Resilience</c> configuration section. Provides named policy presets that
/// can be applied to HttpClient registrations or manual pipeline builders
/// (Infrastructure Deployment §3, Development Standards §18).
/// </summary>
public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public RetryPolicyOptions Retry { get; set; } = new();
    public TimeoutPolicyOptions Timeout { get; set; } = new();
    public CircuitBreakerPolicyOptions CircuitBreaker { get; set; } = new();
}

public sealed class RetryPolicyOptions
{
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(100, 60000)]
    public int BaseDelayMs { get; set; } = 1000;

    [Range(1, 30000)]
    public int MaxDelayMs { get; set; } = 30000;

    public bool UseJitter { get; set; } = true;
}

public sealed class TimeoutPolicyOptions
{
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class CircuitBreakerPolicyOptions
{
    [Range(1, 1000)]
    public int FailureThreshold { get; set; } = 5;

    [Range(1, 600)]
    public int BreakDurationSeconds { get; set; } = 30;

    [Range(1, 10000)]
    public int SamplingDurationSeconds { get; set; } = 60;

    [Range(0.0, 1.0)]
    public double FailureRatio { get; set; } = 0.5;

    [Range(2, 10000)]
    public int MinimumThroughput { get; set; } = 10;
}
