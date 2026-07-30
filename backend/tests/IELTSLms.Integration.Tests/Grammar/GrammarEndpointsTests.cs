using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Grammar;

/// <summary>
/// End-to-end checks for the Grammar endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="Billing.BillingEndpointsTests"/>.
/// </summary>
public sealed class GrammarEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetGrammarTopics_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/grammar-topics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGrammarTopic_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/grammar-topics/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateGrammarTopic_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/grammar-topics", new { title = "x", description = "x", track = "Academic", difficulty = "Beginner" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitGrammarAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/grammar-exercises/{Guid.NewGuid()}/attempts", new { answers = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGrammarProgress_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}/grammar-progress");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
