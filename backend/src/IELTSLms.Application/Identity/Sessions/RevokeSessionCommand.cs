using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Sessions;

/// <summary>
/// Revokes one of the caller's device sessions and its refresh-token family (Auth §4.2, §4.3). The
/// session must belong to the calling user; a mismatch or missing session is treated as already
/// revoked and still succeeds, so the endpoint never leaks another user's session state.
/// </summary>
/// <param name="UserId">The authenticated user, from the access-token subject.</param>
/// <param name="SessionId">The session to revoke.</param>
public sealed record RevokeSessionCommand(Guid UserId, Guid SessionId) : ICommand;
