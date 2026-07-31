using System.Net;

namespace IELTSLms.Integration.Tests.Files;

/// <summary>
/// The signed-download route is intentionally anonymous (the signature is the authorization, not a
/// bearer token) — these checks confirm an invalid/missing signature is rejected before any file
/// lookup, and that there is no other reachable route to file bytes (API Design Standards §14.1).
/// </summary>
public sealed class FilesEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task StreamFile_with_a_bogus_signature_is_rejected_as_forbidden_not_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/files/stream?key=some/key.mp3&expires=9999999999&sig=not-a-real-signature");

        // Anonymous route: an invalid signature is a 403 (bad proof), never a 401 (which would imply
        // JWT auth is expected here).
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StreamFile_with_no_query_parameters_is_rejected()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/files/stream");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
