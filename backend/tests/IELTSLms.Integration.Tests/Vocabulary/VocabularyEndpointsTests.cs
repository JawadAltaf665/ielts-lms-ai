using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.Vocabulary;

/// <summary>
/// End-to-end checks for the Vocabulary endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, mirroring <see cref="Grammar.GrammarEndpointsTests"/>.
/// </summary>
public sealed class VocabularyEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetVocabularyWords_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/vocabulary-words");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVocabularyWord_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/vocabulary-words/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateVocabularyWord_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/vocabulary-words",
            new { word = "x", sense = "x", meaning = "x", examples = Array.Empty<string>(), frequencyBand = "High", difficultyLevel = "Beginner" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNextVocabularyReviewBatch_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/vocabulary-review-sessions/next");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GradeVocabularyReview_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/vocabulary-review-sessions/{Guid.NewGuid()}/grade", new { grade = 4 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVocabularyProgress_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}/vocabulary-progress");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
