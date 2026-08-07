using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Speaking;

/// <summary>
/// End-to-end checks for the Speaking endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="Writing.WritingEndpointsTests"/>.
/// </summary>
public sealed class SpeakingEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetSpeakingTasks_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/speaking-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeakingTask_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/speaking-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSpeakingTask_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/speaking-tasks",
            new { title = "Part 2", part = "Part2", promptText = "Describe a place.", categoryId = (Guid?)null });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSpeakingAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent
        {
            { new StringContent(Guid.NewGuid().ToString()), "speakingTaskId" },
            { new StringContent("45"), "durationSeconds" },
        };

        var response = await client.PostAsync("/api/v1/speaking-attempts", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSpeakingAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/speaking-attempts/{Guid.NewGuid()}/submit", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeakingAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/speaking-attempts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeakingEvaluation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/speaking-evaluations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OverrideSpeakingEvaluation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/speaking-evaluations/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new
            {
                fluencyCoherence = 6.0, lexicalResource = 6.0, grammaticalRangeAccuracy = 6.0, pronunciation = 6.0, reason = "r",
            }),
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSpeakingReviewQueue_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/speaking-evaluations/review-queue");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResolveSpeakingEvaluationReview_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/speaking-evaluations/{Guid.NewGuid()}/resolve-review", new { decision = "Confirmed" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
