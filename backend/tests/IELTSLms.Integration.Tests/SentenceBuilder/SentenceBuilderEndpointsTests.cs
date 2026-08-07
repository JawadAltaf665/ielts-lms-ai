using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.SentenceBuilder;

/// <summary>
/// End-to-end checks for the Sentence Builder endpoints that don't require a live database:
/// authorization is enforced by middleware before any handler/EF access, mirroring
/// <see cref="Grammar.GrammarEndpointsTests"/>.
/// </summary>
public sealed class SentenceBuilderEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetSentenceExercises_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/sentence-exercises");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSentenceExercise_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/sentence-exercises/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static readonly string[] SampleTargetPattern = ["although"];

    [Fact]
    public async Task CreateSentenceExercise_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/sentence-exercises", new { prompt = "x", targetPattern = SampleTargetPattern, difficulty = "Beginner" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSentenceAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/sentence-exercises/{Guid.NewGuid()}/attempts", new { submittedText = "x" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSentenceAttempt_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/sentence-attempts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGrammarAttemptCoaching_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/grammar-attempts/{Guid.NewGuid()}/coaching");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestVocabularyCoaching_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/ai/vocabulary-coach", new { word = "ubiquitous" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
