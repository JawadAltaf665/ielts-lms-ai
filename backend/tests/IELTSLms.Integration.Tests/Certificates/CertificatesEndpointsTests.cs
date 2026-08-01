using System.Net;

namespace IELTSLms.Integration.Tests.Certificates;

/// <summary>
/// End-to-end checks for the Certificates endpoints, mirroring <see cref="AI.AiEndpointsTests"/> for
/// the authenticated route. <see cref="VerifyCertificate_is_reachable_without_authentication"/> checks
/// the phase's one deliberate public exception (API Design Standards §16) instead of the usual
/// unauthenticated-returns-401 shape.
/// </summary>
public sealed class CertificatesEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetCertificate_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/certificates/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task VerifyCertificate_is_reachable_without_authentication()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/certificates/verify/NONEXISTENT-CODE");

        // The one deliberate public/unauthenticated exception in the whole phase (API Design
        // Standards §16) — this must never be 401, unlike every other endpoint in this test suite.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
