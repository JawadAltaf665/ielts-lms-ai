using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>Aggregate access for <see cref="UserSession"/> and per-user session listings.</summary>
public interface IUserSessionRepository : IRepository<UserSession, UserSessionId>
{
    /// <summary>Loads the active (non-revoked, unexpired) sessions for a user, most-recent first.</summary>
    Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(
        UserId userId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
