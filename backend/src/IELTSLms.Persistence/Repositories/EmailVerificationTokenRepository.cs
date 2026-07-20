using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="IEmailVerificationTokenRepository"/>: lookup by stored token hash.</summary>
internal sealed class EmailVerificationTokenRepository(ApplicationDbContext dbContext)
    : RepositoryBase<EmailVerificationToken, EmailVerificationTokenId>(dbContext), IEmailVerificationTokenRepository
{
    /// <inheritdoc />
    public async Task<EmailVerificationToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken).ConfigureAwait(false);
}
