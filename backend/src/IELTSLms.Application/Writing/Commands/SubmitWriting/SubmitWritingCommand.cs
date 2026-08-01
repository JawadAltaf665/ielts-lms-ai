using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Writing.Commands.SubmitWriting;

/// <summary>
/// Submits a response to a <see cref="Domain.Writing.WritingTask"/> for AI evaluation. There is no
/// client-supplied word count field at all — <c>ResponseText</c> is the only input, and the handler
/// always recomputes the count server-side (sprint requirement: never trust a client-supplied count).
/// Returns <c>202 Accepted</c> plus an operation resource; the evaluation itself runs asynchronously
/// via <c>WritingEvaluationJob</c>.
/// </summary>
public sealed record SubmitWritingCommand(Guid TenantId, Guid UserId, Guid WritingTaskId, string ResponseText)
    : ICommand<WritingSubmissionAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.submit"];
}

public sealed record WritingSubmissionAcceptedResponse(Guid SubmissionId, Guid OperationId, string Status);
