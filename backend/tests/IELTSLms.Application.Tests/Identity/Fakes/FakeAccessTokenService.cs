using IELTSLms.Abstractions.Security;

namespace IELTSLms.Application.Tests.Identity.Fakes;

/// <summary>Captures the last request it was asked to mint a token for, so tests can assert on its claims.</summary>
internal sealed class FakeAccessTokenService : IAccessTokenService
{
    public AccessTokenRequest? LastRequest { get; private set; }

    public AccessToken CreateToken(AccessTokenRequest request)
    {
        LastRequest = request;
        return new AccessToken("fake-jwt", DateTimeOffset.UtcNow.AddMinutes(15));
    }
}
