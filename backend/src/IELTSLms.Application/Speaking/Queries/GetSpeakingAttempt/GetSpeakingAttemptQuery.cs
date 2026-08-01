using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingAttempt;

/// <summary>Gets one of the caller's own attempts, or (Teacher role) any attempt in the tenant —
/// checked manually via <c>ICurrentUserService.Roles</c> in the handler, same shape as
/// <c>GetWritingSubmissionQuery</c>.</summary>
public sealed record GetSpeakingAttemptQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<SpeakingAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.submit"];
}
