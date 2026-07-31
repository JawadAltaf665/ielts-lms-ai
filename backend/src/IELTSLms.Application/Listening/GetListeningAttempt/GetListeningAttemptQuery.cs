using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.SubmitListeningAttempt;

namespace IELTSLms.Application.Listening.GetListeningAttempt;

/// <summary>
/// Gets one of the caller's own past attempts and its result. <c>listening.attempt</c> (own) covers
/// the self-service path; cohort-level viewing (<c>analytics.view.cohort</c>) is declared for
/// documentation only — not enforced yet, same as every other fine-grained permission in this codebase.
/// </summary>
public sealed record GetListeningAttemptQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<ListeningAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.attempt"];
}
