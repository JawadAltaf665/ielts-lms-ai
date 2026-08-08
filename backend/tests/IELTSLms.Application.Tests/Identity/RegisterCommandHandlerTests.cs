using IELTSLms.Application.Identity;
using IELTSLms.Application.Identity.Authorization;
using IELTSLms.Application.Identity.Register;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Identity.Fakes;
using IELTSLms.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Tests.Identity;

public sealed class RegisterCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private (RegisterCommandHandler Handler, FakeUserRepository Users, FakeUserRoleRepository UserRoles)
        CreateHandler(Role? seededStudentRole)
    {
        var users = new FakeUserRepository();
        var roles = new FakeRoleRepository();
        if (seededStudentRole is not null)
        {
            roles.Seed(seededStudentRole);
        }

        var roleNamesById = new Dictionary<RoleId, string>();
        if (seededStudentRole is not null)
        {
            roleNamesById[seededStudentRole.Id] = seededStudentRole.Name;
        }

        var userRoles = new FakeUserRoleRepository(id => roleNamesById.GetValueOrDefault(id));

        var handler = new RegisterCommandHandler(
            users,
            roles,
            userRoles,
            new Fakes.FakeEmailVerificationTokenRepository(),
            new FakePasswordHasher(),
            new FakeSecureTokenGenerator(),
            new Fakes.FakeEmailSender(),
            new Fakes.FakeEmailTemplateRenderer(),
            new FakeUnitOfWork(),
            new FakeClock(Now),
            new FakeGuidProvider(),
            new FakeTenantContext(_tenantId),
            Options.Create(new AuthOptions()),
            NullLogger<RegisterCommandHandler>.Instance);

        return (handler, users, userRoles);
    }

    [Fact]
    public async Task Assigns_the_default_student_role_to_a_new_self_registered_account()
    {
        var studentRole = Role.CreateSystemRole(RoleId.From(Guid.NewGuid()), RbacCatalog.DefaultSelfRegistrationRole, "desc", Now);
        var (handler, users, userRoles) = CreateHandler(studentRole);

        var result = await handler.Handle(
            new RegisterCommand("new.learner@example.com", "Correct-Horse-9!", "New Learner"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var createdUser = Assert.Single(users.GetAllForTest());
        var assignment = Assert.Single(userRoles.Assignments);
        Assert.Equal(createdUser.Id, assignment.UserId);
        Assert.Equal(studentRole.Id, assignment.RoleId);
        Assert.Equal(_tenantId, assignment.TenantId);
    }

    [Fact]
    public async Task Registration_still_succeeds_when_the_default_role_is_not_yet_seeded()
    {
        // Degrade gracefully (RegisterCommandHandler.DefaultRoleMissing) rather than failing
        // registration outright if RbacCatalogSeeder hasn't run yet.
        var (handler, users, userRoles) = CreateHandler(seededStudentRole: null);

        var result = await handler.Handle(
            new RegisterCommand("another.learner@example.com", "Correct-Horse-9!", "Another Learner"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(users.GetAllForTest());
        Assert.Empty(userRoles.Assignments);
    }
}
