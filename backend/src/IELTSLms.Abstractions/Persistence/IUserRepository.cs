using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Aggregate access for <see cref="User"/>, plus the identity-specific lookups the auth flows need
/// (Backend Architecture §5.5). Cross-aggregate read models belong in query handlers, not here.
/// </summary>
public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>Loads a user by normalized email, or <see langword="null"/> if none exists.</summary>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>Returns whether an account already exists for the normalized email.</summary>
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
}
