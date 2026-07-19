using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>Aggregate access for <see cref="RefreshToken"/> and its rotation/revocation queries.</summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, RefreshTokenId>
{
    /// <summary>Loads a refresh token by its stored hash, or <see langword="null"/> if not found.</summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Loads all tokens in a session's family, for family-wide revocation (Auth §4.3).</summary>
    Task<IReadOnlyList<RefreshToken>> GetBySessionAsync(UserSessionId sessionId, CancellationToken cancellationToken = default);
}
