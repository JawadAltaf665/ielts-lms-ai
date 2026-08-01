using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Writing.Queries.GetWritingSubmission;

/// <summary>
/// Gets one of the caller's own submissions, or (Teacher role) any submission in the tenant — checked
/// manually via <c>ICurrentUserService.Roles</c> in the handler, same shape as the ownership check on
/// <c>SubmitReadingAttemptCommandHandler</c> (NotFound, never Forbidden, for a submission that exists
/// but isn't the caller's and the caller isn't a Teacher).
/// </summary>
public sealed record GetWritingSubmissionQuery(Guid TenantId, Guid UserId, Guid SubmissionId)
    : IQuery<WritingSubmissionResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.submit"];
}

public sealed record WritingSubmissionResponse(
    Guid Id, Guid UserId, Guid WritingTaskId, string ResponseText, int WordCount, string Status,
    DateTimeOffset? SubmittedAtUtc, Guid? WritingEvaluationId);
