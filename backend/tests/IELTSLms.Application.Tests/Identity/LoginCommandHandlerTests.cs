using IELTSLms.Application.Identity;
using IELTSLms.Application.Identity.Login;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Identity.Fakes;
using IELTSLms.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Tests.Identity;

public sealed class LoginCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();
    private const string Password = "Correct-Horse-9!";

    private (LoginCommandHandler Handler, FakeAccessTokenService AccessTokens) CreateHandler(
        User user, IReadOnlyList<(RoleId Id, string Name)> assignedRoles)
    {
        var users = new FakeUserRepository();
        users.Seed(user);

        var roleNamesById = assignedRoles.ToDictionary(r => r.Id, r => r.Name);
        var userRoles = new FakeUserRoleRepository(id => roleNamesById.GetValueOrDefault(id));
        foreach (var role in assignedRoles)
        {
            userRoles.Add(UserRole.Assign(user.Id, role.Id, _tenantId, Now, createdByUserId: Guid.Empty));
        }

        var accessTokens = new FakeAccessTokenService();

        var handler = new LoginCommandHandler(
            users,
            new FakeUserSessionRepository(),
            new FakeRefreshTokenRepository(),
            new FakeLoginHistoryRepository(),
            userRoles,
            new FakePasswordHasher(),
            new FakeSecureTokenGenerator(),
            accessTokens,
            new FakeTenantContext(_tenantId),
            new FakeUnitOfWork(),
            new FakeClock(Now),
            new FakeGuidProvider(),
            Options.Create(new AuthOptions()),
            NullLogger<LoginCommandHandler>.Instance);

        return (handler, accessTokens);
    }

    private User CreateVerifiedUser(string email)
    {
        var hasher = new FakePasswordHasher();
        var user = User.Register(
            UserId.From(Guid.NewGuid()),
            _tenantId,
            Email.Create(email).Value,
            PasswordHash.FromHashed(hasher.Hash(Password)),
            "A Learner",
            Now);
        user.VerifyEmail(Now);
        return user;
    }

    [Fact]
    public async Task Mints_an_access_token_carrying_the_users_assigned_roles()
    {
        var user = CreateVerifiedUser("student@example.com");
        var studentRoleId = RoleId.From(Guid.NewGuid());
        var (handler, accessTokens) = CreateHandler(user, [(studentRoleId, "Student")]);

        var result = await handler.Handle(
            new LoginCommand(user.Email.Value, Password, "127.0.0.1", "test-agent"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(accessTokens.LastRequest);
        Assert.Equal(["Student"], accessTokens.LastRequest!.Value.Roles);
        Assert.Equal(user.Id.Value, accessTokens.LastRequest.Value.UserId);
        Assert.Equal(_tenantId, accessTokens.LastRequest.Value.TenantId);
    }

    [Fact]
    public async Task A_user_with_no_role_assignments_gets_an_empty_roles_claim()
    {
        var user = CreateVerifiedUser("norole@example.com");
        var (handler, accessTokens) = CreateHandler(user, []);

        var result = await handler.Handle(
            new LoginCommand(user.Email.Value, Password, "127.0.0.1", "test-agent"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(accessTokens.LastRequest!.Value.Roles);
    }
}
