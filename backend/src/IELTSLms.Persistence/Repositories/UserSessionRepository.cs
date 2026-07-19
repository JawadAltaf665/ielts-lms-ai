using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="IUserSessionRepository"/>: loads a user's active device sessions.</summary>
internal sealed class UserSessionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<UserSession, UserSessionId>(dbContext), IUserSessionRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(
        UserId userId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default) =>
        await Set
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null && s.ExpiresAtUtc > nowUtc && s.AbsoluteExpiresAtUtc > nowUtc)
            .OrderByDescending(s => s.LastSeenAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
