using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// Sprint 9's load-testing acceptance criterion, scoped to what this harness can actually exercise
/// without a live database or a real/mock AI provider (no existing Integration.Tests file in this
/// codebase sets either up — every module's endpoint test only checks the unauthenticated-401 guard,
/// see <see cref="AiEndpointsTests"/>).
///
/// What this proves: the Writing/Speaking submission pipeline (auth middleware → rate limiter →
/// routing) stays responsive and returns consistent results under concurrent burst load, rather than
/// crashing, hanging, or leaking connections — the p95 latency budgets from Testing Strategy §12
/// (Writing ≤30s, Speaking ≤60s) apply to the AI evaluation call itself, which only runs past
/// authentication with a seeded tenant/task and a real or sandboxed AI provider; that requires a
/// staging environment this in-process test harness doesn't have.
///
/// What this does NOT re-prove here: graceful 429 degradation under AI quota exhaustion. That's
/// already covered deterministically by unit tests against the real handler code —
/// <c>SubmitWritingCommandHandlerTests.Quota_exceeded_short_circuits_before_any_job_is_enqueued</c> and
/// <c>RequestBandPredictionCommandHandlerTests.Quota_exceeded_short_circuits_before_any_job_is_enqueued</c>
/// (Application.Tests) — both assert a quota-exhausted request returns <c>RATE_LIMITED_AI_QUOTA_EXCEEDED</c>
/// (429) rather than throwing, without a job ever being enqueued. ponytail: a burst-of-requests-against-
/// a-live-server proof of the *transport-level* rate limiter's 429 behavior would need one of its named
/// policies (Infrastructure/RateLimiting/RateLimitingRegistration.cs) actually attached to a route via
/// <c>RequireRateLimiting</c>/<c>GlobalLimiter</c> — today none are (flagged separately) — so a load test
/// against it here would prove nothing.
/// </summary>
public sealed class AiEndpointLoadTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task Writing_submission_endpoint_stays_responsive_under_concurrent_burst_load()
    {
        using var client = _factory.CreateClient();
        const int concurrentRequests = 50;

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(Enumerable.Range(0, concurrentRequests)
            .Select(_ => client.PostAsJsonAsync("/api/v1/writing-submissions", new { writingTaskId = Guid.NewGuid(), responseText = "x" })));
        stopwatch.Stop();

        // No exceptions, no hangs, and every request got a definite (unauthenticated) response —
        // the pipeline degrades to a fast, consistent rejection rather than failing open or timing out.
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode));
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(30),
            $"{concurrentRequests} concurrent requests took {stopwatch.Elapsed.TotalSeconds:0.0}s; expected well under the Writing p95 budget (Testing Strategy §12: 30s) even for auth-rejected requests.");
    }

    [Fact]
    public async Task Speaking_submission_endpoint_stays_responsive_under_concurrent_burst_load()
    {
        using var client = _factory.CreateClient();
        const int concurrentRequests = 50;

        var stopwatch = Stopwatch.StartNew();
        var responses = await Task.WhenAll(Enumerable.Range(0, concurrentRequests)
            .Select(_ => client.PostAsJsonAsync(
                "/api/v1/speaking-attempts", new { speakingTaskId = Guid.NewGuid(), fileId = Guid.NewGuid(), durationSeconds = 30 })));
        stopwatch.Stop();

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode));
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(60),
            $"{concurrentRequests} concurrent requests took {stopwatch.Elapsed.TotalSeconds:0.0}s; expected well under the Speaking p95 budget (Testing Strategy §12: 60s) even for auth-rejected requests.");
    }
}
