using IELTSLms.Abstractions.Context;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }

    public string? Email { get; set; }

    public bool IsAuthenticated { get; set; } = true;

    public bool IsSystem { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];
}
