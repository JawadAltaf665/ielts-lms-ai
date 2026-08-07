using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// End-to-end checks for the AI Teacher endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="ConversationEndpointsTests"/>.
/// Entitlement/quota/tenant-isolation and the step-state-machine behavior are covered at the
/// Application.Tests/Domain.Tests level, consistent with this suite's existing scope.
/// </summary>
public sealed class TeacherEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task StartLessonSession_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/ai/teacher/lessons/{Guid.NewGuid()}/sessions", new { topic = "Conditionals" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RespondToLessonStep_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/ai/teacher/sessions/{Guid.NewGuid()}/respond", new { message = "OK" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLessonSession_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/ai/teacher/sessions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
