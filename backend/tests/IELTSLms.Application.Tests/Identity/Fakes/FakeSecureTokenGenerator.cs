using IELTSLms.Abstractions.Security;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeSecureTokenGenerator : ISecureTokenGenerator
{
    private int _counter;

    public SecureToken Generate()
    {
        var raw = $"raw-token-{++_counter}";
        return new SecureToken(raw, Hash(raw));
    }

    public string Hash(string rawToken) => "hash:" + rawToken;
}
