using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Certificates.Fakes;

/// <summary>Always "not found" — sufficient for tests that only exercise eligibility/idempotency, not the PDF's learner-name text.</summary>
internal sealed class FakeUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);

    public void Add(User aggregate)
    {
    }

    public void Remove(User aggregate)
    {
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult<User?>(null);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
