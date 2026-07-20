using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IELTSLms.Integration.Tests;

/// <summary>
/// Boots the real API host through <see cref="WebApplicationFactory{TEntryPoint}"/> and exercises
/// the composition root end to end: DI validation, diagnostics endpoints, liveness, and the
/// RFC 7807 Problem Details shape for unmatched routes (API Design Standards §9).
/// </summary>
public sealed class ApiSmokeTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public void Host_builds_and_validates_di()
    {
        // Creating the client forces the host (and DI container) to build; a broken graph throws here.
        using var client = _factory.CreateClient();

        Assert.NotNull(client);
    }

    [Fact]
    public async Task Liveness_probe_returns_healthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Version_endpoint_reports_service_metadata()
    {
        using var client = _factory.CreateClient();

        var payload = await client.GetFromJsonAsync<JsonElement>("/api/v1/version");

        Assert.Equal("IELTS LMS AI API", payload.GetProperty("service").GetString());
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("version").GetString()));
    }

    [Fact]
    public async Task Unmatched_route_returns_problem_details()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Response_echoes_a_correlation_id()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/version");

        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task Client_supplied_correlation_id_is_preserved()
    {
        using var client = _factory.CreateClient();
        const string correlationId = "test-correlation-123";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/version");
        request.Headers.Add("X-Correlation-Id", correlationId);

        var response = await client.SendAsync(request);

        Assert.Equal(correlationId, response.Headers.GetValues("X-Correlation-Id").Single());
    }
}
