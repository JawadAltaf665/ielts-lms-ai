using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class GetWritingEvaluationQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (WritingSubmission Submission, WritingEvaluation Evaluation) SeedEvaluated(
        FakeWritingSubmissionRepository submissions, FakeWritingEvaluationRepository evaluations, Guid tenantId, Guid userId)
    {
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "essay text", Now).Value;
        submission.Submit(200, Now.AddMinutes(1));

        var evaluation = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, submission.Id, 7.0m, 6.5m, 7.0m, 6.5m, 0.9m, "feedback", 0.7m, null, 0.6m,
            null, null, Now.AddMinutes(2)).Value;
        submission.AttachEvaluation(evaluation.Id, evaluation.EstimatedBand, Now.AddMinutes(2));

        submissions.Seed(submission);
        evaluations.Seed(evaluation);
        return (submission, evaluation);
    }

    [Fact]
    public async Task Owner_can_view_their_own_evaluation()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var evaluations = new FakeWritingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var (_, evaluation) = SeedEvaluated(submissions, evaluations, tenantId, userId);
        var handler = new GetWritingEvaluationQueryHandler(evaluations, submissions, new FakeCurrentUserService());

        var result = await handler.Handle(new GetWritingEvaluationQuery(tenantId, userId, evaluation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7.0m, result.Value.EstimatedBand);
    }

    [Fact]
    public async Task Another_users_evaluation_is_not_found_rather_than_forbidden()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var evaluations = new FakeWritingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var (_, evaluation) = SeedEvaluated(submissions, evaluations, tenantId, Guid.NewGuid());
        var handler = new GetWritingEvaluationQueryHandler(evaluations, submissions, new FakeCurrentUserService());

        var result = await handler.Handle(new GetWritingEvaluationQuery(tenantId, Guid.NewGuid(), evaluation.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_WRITING_EVALUATION", result.Error.Code);
    }

    [Fact]
    public async Task A_teacher_can_view_any_evaluation_in_the_tenant()
    {
        var submissions = new FakeWritingSubmissionRepository();
        var evaluations = new FakeWritingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var (_, evaluation) = SeedEvaluated(submissions, evaluations, tenantId, Guid.NewGuid());
        var handler = new GetWritingEvaluationQueryHandler(evaluations, submissions, new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(new GetWritingEvaluationQuery(tenantId, Guid.NewGuid(), evaluation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
