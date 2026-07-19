using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="IUserRepository"/>: aggregate access plus email lookups for auth flows.</summary>
internal sealed class UserRepository(ApplicationDbContext dbContext)
    : RepositoryBase<User, UserId>(dbContext), IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(u => u.Email == Email.FromTrusted(normalizedEmail), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(u => u.Email == Email.FromTrusted(normalizedEmail), cancellationToken)
            .ConfigureAwait(false);
}
