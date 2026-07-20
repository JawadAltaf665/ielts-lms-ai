using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Logout;

/// <summary>
/// Ends a single device session and revokes its refresh-token family (Auth §4.3). The command is
/// idempotent: revoking an already-revoked or unknown session still succeeds so logout never leaks
/// session state.
/// </summary>
/// <param name="UserId">The authenticated user, from the access-token subject.</param>
/// <param name="SessionId">The session to end, from the access-token <c>sid</c> claim.</param>
public sealed record LogoutCommand(Guid UserId, Guid SessionId) : ICommand;
