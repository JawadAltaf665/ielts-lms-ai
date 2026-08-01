using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Writing;

/// <summary>
/// End-to-end checks for the Writing endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="Listening.ListeningEndpointsTests"/>.
/// </summary>
public sealed class WritingEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetWritingTasks_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/writing-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWritingTask_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/writing-tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWritingTask_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/writing-tasks",
            new
            {
                title = "Task 2", taskType = "Task2", track = "Academic", promptText = "Discuss.", rubricReference = (string?)null,
                minWordCount = 250, maxWordCount = 400, categoryId = (Guid?)null,
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitWriting_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/writing-submissions", new { writingTaskId = Guid.NewGuid(), responseText = "My essay." });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWritingSubmission_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/writing-submissions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWritingEvaluation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/writing-evaluations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OverrideWritingEvaluation_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/writing-evaluations/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new { taskResponse = 6.0, coherenceCohesion = 6.0, lexicalResource = 6.0, grammaticalRangeAccuracy = 6.0, reason = "r" }),
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
