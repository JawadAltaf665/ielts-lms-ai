using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace IELTSLms.Persistence.Repositories;

internal sealed class UserInvitationRepository(ApplicationDbContext dbContext) 
    : RepositoryBase<UserInvitation, UserInvitationId>(dbContext), IUserInvitationRepository
{
    public async Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }
}
