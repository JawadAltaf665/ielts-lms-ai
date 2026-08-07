using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// End-to-end checks for the Prompt Library &amp; Versioning admin endpoints that don't require a live
/// database: authorization is enforced by middleware before any handler/EF access, mirroring
/// <see cref="AiEndpointsTests"/>.
/// </summary>
public sealed class PromptEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task ListPromptTemplates_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/ai/prompts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPromptTemplate_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/ai/prompts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePromptVersion_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/ai/prompts", new { feature = "grammar-coach", scope = "default", systemTemplate = "x" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ActivatePromptVersion_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/ai/prompts/{Guid.NewGuid()}/activate", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RollbackPromptVersion_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/ai/prompts/{Guid.NewGuid()}/rollback", new { reason = "x" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RetirePromptVersion_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/v1/ai/prompts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
