using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Listening;

/// <summary>
/// End-to-end checks for the Listening endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="Reading.ReadingEndpointsTests"/>.
/// </summary>
public sealed class ListeningEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetListeningAudios_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/listening-audios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetListeningAudio_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/listening-audios/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetListeningAudioStream_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/listening-audios/{Guid.NewGuid()}/stream");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateListeningAudio_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent { { new StringContent("Section 1"), "title" } };

        var response = await client.PostAsync("/api/v1/listening-audios", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static readonly string[] SingleAnswerKey = ["x"];

    [Fact]
    public async Task AddListeningQuestion_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/listening-audios/{Guid.NewGuid()}/questions",
            new { type = "ShortAnswer", prompt = "x", answerKey = SingleAnswerKey, sequence = 0 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StartListeningAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/listening-attempts", new { audioId = Guid.NewGuid(), mode = "Exam" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterListeningReplay_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/listening-attempts/{Guid.NewGuid()}/replays", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitListeningAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/listening-attempts/{Guid.NewGuid()}/submit", new { answers = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetListeningAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/listening-attempts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
