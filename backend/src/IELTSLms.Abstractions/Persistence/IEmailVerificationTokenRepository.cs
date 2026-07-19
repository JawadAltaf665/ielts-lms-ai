using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>Aggregate access for <see cref="EmailVerificationToken"/> with lookup by stored hash.</summary>
public interface IEmailVerificationTokenRepository
    : IRepository<EmailVerificationToken, EmailVerificationTokenId>
{
    /// <summary>Loads a verification token by its stored hash, or <see langword="null"/> if not found.</summary>
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
