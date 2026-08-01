using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Queries.GetWritingSubmission;

public sealed class GetWritingSubmissionQueryHandler(IWritingSubmissionRepository submissions, ICurrentUserService currentUser)
    : IQueryHandler<GetWritingSubmissionQuery, WritingSubmissionResponse>
{
    public async Task<Result<WritingSubmissionResponse>> Handle(GetWritingSubmissionQuery request, CancellationToken cancellationToken)
    {
        var submission = await submissions
            .GetByIdAsync(WritingSubmissionId.From(request.SubmissionId), cancellationToken)
            .ConfigureAwait(false);

        var isOwner = submission is not null && submission.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (submission is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<WritingSubmissionResponse>(WritingErrors.SubmissionNotFound);
        }

        return Result.Success(ToResponse(submission));
    }

    internal static WritingSubmissionResponse ToResponse(WritingSubmission submission) => new(
        submission.Id.Value, submission.UserId, submission.WritingTaskId.Value, submission.ResponseText, submission.WordCount,
        submission.Status.ToString(), submission.SubmittedAtUtc, submission.WritingEvaluationId?.Value);
}
