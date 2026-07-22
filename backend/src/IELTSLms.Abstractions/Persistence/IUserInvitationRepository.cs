using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

public interface IUserInvitationRepository : IRepository<UserInvitation, UserInvitationId>
{
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
}
