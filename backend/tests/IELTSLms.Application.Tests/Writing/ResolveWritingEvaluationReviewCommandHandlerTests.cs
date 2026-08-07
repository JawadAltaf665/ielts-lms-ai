using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Commands.ResolveWritingEvaluationReview;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class ResolveWritingEvaluationReviewCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const decimal LowConfidenceThreshold = 0.7m;
    private const decimal HighSimilarityThreshold = 0.6m;

    private static WritingEvaluation Flagged(Guid tenantId) =>
        WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            0.4m, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, Now).Value;

    private static ResolveWritingEvaluationReviewCommandHandler CreateHandler(
        FakeWritingEvaluationRepository evaluations, FakeUnitOfWork unitOfWork, params string[] roles) =>
        new(evaluations, unitOfWork, new FakeClock(Now.AddDays(1)), new FakeCurrentUserService { Roles = roles });

    [Fact]
    public async Task Teacher_resolves_a_flagged_evaluation_and_it_is_audited()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();
        var evaluation = Flagged(tenantId);
        evaluations.Seed(evaluation);
        var teacherId = Guid.NewGuid();

        var handler = CreateHandler(evaluations, new FakeUnitOfWork(), "Teacher");
        var result = await handler.Handle(
            new ResolveWritingEvaluationReviewCommand(tenantId, teacherId, evaluation.Id.Value, "Confirmed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsReviewed);
        Assert.Equal("Confirmed", result.Value.ReviewDecision);
        Assert.Equal(teacherId, result.Value.ReviewedBy);
        Assert.Equal(Now.AddDays(1), result.Value.ReviewedAtUtc);
    }

    [Fact]
    public async Task Non_teacher_is_rejected()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();
        var evaluation = Flagged(tenantId);
        evaluations.Seed(evaluation);

        var handler = CreateHandler(evaluations, new FakeUnitOfWork(), "Student");
        var result = await handler.Handle(
            new ResolveWritingEvaluationReviewCommand(tenantId, Guid.NewGuid(), evaluation.Id.Value, "Confirmed"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_WRITING_EVALUATION_RESOLVE_REVIEW_TEACHER_ONLY", result.Error.Code);
        Assert.False(evaluation.IsReviewed);
    }

    [Fact]
    public async Task Unknown_evaluation_is_not_found()
    {
        var evaluations = new FakeWritingEvaluationRepository();
        var handler = CreateHandler(evaluations, new FakeUnitOfWork(), "Teacher");

        var result = await handler.Handle(
            new ResolveWritingEvaluationReviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Confirmed"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_WRITING_EVALUATION", result.Error.Code);
    }

    [Fact]
    public async Task Resolving_an_evaluation_not_flagged_for_review_fails()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();
        var notFlagged = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            0.9m, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, Now).Value;
        evaluations.Seed(notFlagged);

        var handler = CreateHandler(evaluations, new FakeUnitOfWork(), "Teacher");
        var result = await handler.Handle(
            new ResolveWritingEvaluationReviewCommand(tenantId, Guid.NewGuid(), notFlagged.Id.Value, "Confirmed"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_WRITING_EVALUATION_NOT_FLAGGED_FOR_REVIEW", result.Error.Code);
    }
}
