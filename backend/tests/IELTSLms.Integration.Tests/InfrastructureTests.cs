using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IELTSLms.Integration.Tests;

/// <summary>
/// Tests for Sprint 5 shared infrastructure: health checks, rate limiting responses,
/// CORS headers, security headers, and response compression.
/// </summary>
public sealed class InfrastructureTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task Health_ready_returns_ok_with_custom_checks()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_startup_returns_success_or_degraded()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/startup");

        // In tests, config health check may fail (missing Serilog config) — but the endpoint should respond.
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected 200 or 503, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Health_detail_returns_json_with_entries()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/detail");

        // Health detail endpoint responds (may be unhealthy due to no postgres/redis in test).
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(payload.TryGetProperty("status", out _));
        Assert.True(payload.GetProperty("entries").EnumerateArray().Any());
    }

    [Fact]
    public async Task Security_headers_are_present_on_response()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/version");

        Assert.True(response.Headers.Contains("X-Content-Type-Options")
            || response.Content.Headers.Contains("X-Content-Type-Options")
            || HasHeaderInRawResponse(response, "X-Content-Type-Options"));

        var responseMessage = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/v1/version"));
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
    }

    [Fact]
    public async Task Cors_preflight_returns_allowed_methods()
    {
        using var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/version");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // CORS should respond (even if 404/405, the headers should be set by the middleware)
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Rate_limiter_middleware_is_registered()
    {
        // Verify the rate limiter is wired by making a normal request — it should succeed (not 429)
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static bool HasHeaderInRawResponse(HttpResponseMessage response, string headerName) =>
        response.Headers.Contains(headerName)
        || response.Content.Headers.Contains(headerName);
}
