using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Queries.GetWritingSubmission;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class GetWritingSubmissionQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static WritingSubmission SeedSubmitted(FakeWritingSubmissionRepository submissions, Guid tenantId, Guid userId)
    {
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "essay text", Now).Value;
        submission.Submit(200, Now.AddMinutes(1));
        submissions.Seed(submission);
        return submission;
    }

    [Fact]
    public async Task Owner_can_view_their_own_submission()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var submission = SeedSubmitted(submissions, tenantId, userId);
        var handler = new GetWritingSubmissionQueryHandler(submissions, new FakeCurrentUserService());

        var result = await handler.Handle(new GetWritingSubmissionQuery(tenantId, userId, submission.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(submission.Id.Value, result.Value.Id);
    }

    [Fact]
    public async Task Another_users_submission_is_not_found_rather_than_forbidden()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var tenantId = Guid.NewGuid();
        var submission = SeedSubmitted(submissions, tenantId, Guid.NewGuid());
        var handler = new GetWritingSubmissionQueryHandler(submissions, new FakeCurrentUserService());

        var result = await handler.Handle(new GetWritingSubmissionQuery(tenantId, Guid.NewGuid(), submission.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_WRITING_SUBMISSION", result.Error.Code);
    }

    [Fact]
    public async Task A_teacher_can_view_any_submission_in_the_tenant()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var tenantId = Guid.NewGuid();
        var submission = SeedSubmitted(submissions, tenantId, Guid.NewGuid());
        var handler = new GetWritingSubmissionQueryHandler(submissions, new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(new GetWritingSubmissionQuery(tenantId, Guid.NewGuid(), submission.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
