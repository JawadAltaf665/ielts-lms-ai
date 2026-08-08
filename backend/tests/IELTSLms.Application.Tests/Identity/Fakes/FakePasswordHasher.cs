using IELTSLms.Abstractions.Security;

namespace IELTSLms.Application.Tests.Identity.Fakes;

/// <summary>Not real hashing — `Hash` prefixes the input, `Verify` checks the prefix. Fast, deterministic, test-only.</summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public bool NextVerifyNeedsRehash { get; set; }

    public string Hash(string password) => "hashed:" + password;

    public PasswordVerificationResult Verify(string password, string encodedHash) =>
        new(encodedHash == Hash(password), NextVerifyNeedsRehash);
}
