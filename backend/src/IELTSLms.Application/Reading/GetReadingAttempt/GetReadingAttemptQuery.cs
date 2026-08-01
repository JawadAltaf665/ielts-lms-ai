using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.SubmitReadingAttempt;

namespace IELTSLms.Application.Reading.GetReadingAttempt;

/// <summary>
/// Gets one of the caller's own past attempts and its result. <c>reading.attempt</c> (own) covers the
/// self-service path; cohort-level viewing (<c>analytics.view.cohort</c>) is declared for documentation
/// only — not enforced yet, same as every other fine-grained permission in this codebase
/// (<c>AuthorizationPipelineBehavior</c> only checks <c>RequiredRoles</c> today).
/// </summary>
public sealed record GetReadingAttemptQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<ReadingAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.attempt"];
}
