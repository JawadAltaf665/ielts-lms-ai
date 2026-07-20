namespace IELTSLms.Abstractions.Context;

/// <summary>
/// Provides the identity of the currently authenticated user within the request scope.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsSystem { get; }
    IReadOnlyList<string> Roles { get; }
}
