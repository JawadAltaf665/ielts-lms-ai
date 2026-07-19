using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="IRefreshTokenRepository"/>: lookup by hash and per-session family loads.</summary>
internal sealed class RefreshTokenRepository(ApplicationDbContext dbContext)
    : RepositoryBase<RefreshToken, RefreshTokenId>(dbContext), IRefreshTokenRepository
{
    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RefreshToken>> GetBySessionAsync(
        UserSessionId sessionId,
        CancellationToken cancellationToken = default) =>
        await Set.Where(t => t.SessionId == sessionId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
