using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

/// <summary>Stateful in-memory fake — Add() actually stores, so Login/Register round-trip correctly.</summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<UserId, User> _byId = [];

    public void Seed(User user) => _byId[user.Id] = user;

    public IReadOnlyCollection<User> GetAllForTest() => _byId.Values;

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(User aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(User aggregate) => _byId.Remove(aggregate.Id);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(u => u.Email.Value == normalizedEmail));

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.Any(u => u.Email.Value == normalizedEmail));
}
