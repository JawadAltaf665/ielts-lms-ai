using IELTSLms.Abstractions.Context;
using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Behaviors;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.Abstractions;

internal sealed record TestRequest(string[] RequiredRoles, string[] RequiredPermissions) : IAuthorizeRequest;

internal sealed class FakePermissionResolver(IReadOnlyList<string> granted) : IPermissionResolver
{
    public Task<IReadOnlyList<string>> GetPermissionsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(granted);
}

public sealed class AuthorizationPipelineBehaviorTests
{
    private static AuthorizationPipelineBehavior<TestRequest, Result> CreateBehavior(
        Guid? userId, string[] roles, string[] grantedPermissions)
    {
        var currentUser = new FakeCurrentUserService { UserId = userId, Roles = roles };
        return new AuthorizationPipelineBehavior<TestRequest, Result>(
            currentUser,
            new FakePermissionResolver(grantedPermissions),
            NullLogger<AuthorizationPipelineBehavior<TestRequest, Result>>.Instance);
    }

    private static RequestHandlerDelegate<Result> NextReturnsSuccess() => () => Task.FromResult(Result.Success());

    [Fact]
    public async Task Allows_a_request_with_no_role_or_permission_requirements()
    {
        var behavior = CreateBehavior(userId: null, roles: [], grantedPermissions: []);

        var result = await behavior.Handle(
            new TestRequest([], []), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Denies_an_anonymous_caller_when_roles_are_required()
    {
        var behavior = CreateBehavior(userId: null, roles: [], grantedPermissions: []);

        var result = await behavior.Handle(
            new TestRequest(["Teacher"], []), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_INSUFFICIENT_ROLE", result.Error.Code);
    }

    [Fact]
    public async Task Denies_a_caller_missing_the_required_role()
    {
        var behavior = CreateBehavior(Guid.NewGuid(), roles: ["Student"], grantedPermissions: []);

        var result = await behavior.Handle(
            new TestRequest(["Teacher"], []), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_INSUFFICIENT_ROLE", result.Error.Code);
    }

    [Fact]
    public async Task Allows_a_caller_holding_one_of_the_required_roles()
    {
        var behavior = CreateBehavior(Guid.NewGuid(), roles: ["Teacher"], grantedPermissions: []);

        var result = await behavior.Handle(
            new TestRequest(["Tenant Admin", "Teacher"], []), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Denies_a_caller_whose_roles_do_not_grant_the_required_permission()
    {
        var behavior = CreateBehavior(Guid.NewGuid(), roles: ["Student"], grantedPermissions: ["writing.submit"]);

        var result = await behavior.Handle(
            new TestRequest([], ["content.manage"]), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_INSUFFICIENT_PERMISSION", result.Error.Code);
    }

    [Fact]
    public async Task Allows_a_caller_whose_roles_grant_the_required_permission()
    {
        var behavior = CreateBehavior(Guid.NewGuid(), roles: ["Student"], grantedPermissions: ["writing.submit"]);

        var result = await behavior.Handle(
            new TestRequest([], ["writing.submit"]), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Requires_both_role_and_permission_when_both_are_declared()
    {
        var behavior = CreateBehavior(Guid.NewGuid(), roles: ["Teacher"], grantedPermissions: []);

        // Has the role but not the permission — must still fail.
        var result = await behavior.Handle(
            new TestRequest(["Teacher"], ["writing.evaluate"]), NextReturnsSuccess(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_INSUFFICIENT_PERMISSION", result.Error.Code);
    }
}
