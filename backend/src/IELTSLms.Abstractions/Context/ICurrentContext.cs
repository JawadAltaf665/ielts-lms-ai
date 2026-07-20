namespace IELTSLms.Abstractions.Context;

/// <summary>
/// Ambient request context — who is calling and which tenant they belong to. Resolved at the API
/// boundary and consumed by application/persistence for authorization and the global tenant filter
/// (Backend Architecture §11). This is a placeholder surface; claims are wired up with auth later.
/// </summary>
public interface ICurrentContext
{
    /// <summary>The authenticated user's id, or <see langword="null"/> for anonymous requests.</summary>
    Guid? UserId { get; }

    /// <summary>The tenant the current request operates within, or <see langword="null"/> if not yet resolved.</summary>
    Guid? TenantId { get; }

    /// <summary><see langword="true"/> when a user identity is present on the request.</summary>
    bool IsAuthenticated { get; }
}
