using System.Net;
using System.Net.Http.Json;

namespace IELTSLms.Integration.Tests.AI;

/// <summary>
/// End-to-end checks for the Sprint 9 Band Prediction endpoints that don't require a live database:
/// authorization is enforced by middleware before any handler/EF access, mirroring <see cref="AiEndpointsTests"/>.
/// </summary>
public sealed class BandPredictionEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task RequestBandPrediction_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/band-predictions", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBandPrediction_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/band-predictions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
