using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeEmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly Dictionary<EmailVerificationTokenId, EmailVerificationToken> _byId = [];

    public Task<EmailVerificationToken?> GetByIdAsync(EmailVerificationTokenId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(EmailVerificationToken aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(EmailVerificationToken aggregate) => _byId.Remove(aggregate.Id);

    public Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(t => t.TokenHash == tokenHash));
}
