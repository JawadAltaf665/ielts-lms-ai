using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// End-to-end checks for the AI Tutor endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="ConversationEndpointsTests"/>.
/// Entitlement/quota/tenant-isolation and the scope-guard behavior are covered at the Application.Tests
/// level, consistent with this suite's existing scope.
/// </summary>
public sealed class TutorEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task StartTutorConversation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/ai/tutor/conversations", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AskTutor_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/ai/tutor/conversations/{Guid.NewGuid()}/messages", new { question = "hi" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
