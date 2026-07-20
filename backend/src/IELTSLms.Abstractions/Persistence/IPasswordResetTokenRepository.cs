using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>Aggregate access for <see cref="PasswordResetToken"/> with lookup by stored hash.</summary>
public interface IPasswordResetTokenRepository
    : IRepository<PasswordResetToken, PasswordResetTokenId>
{
    /// <summary>Loads a reset token by its stored hash, or <see langword="null"/> if not found.</summary>
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
