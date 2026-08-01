using System.Net;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// End-to-end checks for the Memory &amp; Conversation History endpoints that don't require a live
/// database: authorization is enforced by middleware before any handler/EF access, mirroring
/// <see cref="PromptEndpointsTests"/>. Tenant/user isolation-as-404, append-only enforcement, and the
/// summarization concurrency guard are covered at the Application/Domain.Tests level (fakes can't
/// simulate a live optimistic-concurrency race), consistent with this suite's existing scope.
/// </summary>
public sealed class ConversationEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task ListConversations_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/conversations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetConversation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/conversations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveConversation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/v1/conversations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
