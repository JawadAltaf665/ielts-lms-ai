using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Analytics;

/// <summary>
/// End-to-end checks for the Sprint 9 exam-history/review endpoints that don't require a live
/// database: authorization is enforced by middleware before any handler/EF access, mirroring
/// <see cref="AI.AiEndpointsTests"/>.
/// </summary>
public sealed class AnalyticsEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetExamHistory_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}/exam-history");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAttemptReview_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/attempts/writing/{Guid.NewGuid()}/review");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
