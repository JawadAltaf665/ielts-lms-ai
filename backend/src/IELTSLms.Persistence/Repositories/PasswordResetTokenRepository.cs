using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="IPasswordResetTokenRepository"/>: lookup by stored token hash.</summary>
internal sealed class PasswordResetTokenRepository(ApplicationDbContext dbContext)
    : RepositoryBase<PasswordResetToken, PasswordResetTokenId>(dbContext), IPasswordResetTokenRepository
{
    /// <inheritdoc />
    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken).ConfigureAwait(false);
}
