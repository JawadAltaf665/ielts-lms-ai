using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Queries.GetWritingReviewQueue;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class GetWritingReviewQueueQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const decimal LowConfidenceThreshold = 0.7m;
    private const decimal HighSimilarityThreshold = 0.6m;

    private static WritingEvaluation Flagged(Guid tenantId, DateTimeOffset createdAtUtc) =>
        WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            0.4m, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, createdAtUtc).Value;

    private static WritingEvaluation NotFlagged(Guid tenantId, DateTimeOffset createdAtUtc) =>
        WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            0.9m, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, createdAtUtc).Value;

    private static GetWritingReviewQueueQueryHandler CreateHandler(
        FakeWritingEvaluationRepository evaluations, params string[] roles) =>
        new(evaluations, new FakeCurrentUserService { Roles = roles });

    [Fact]
    public async Task Lists_only_flagged_unreviewed_unoverridden_evaluations_in_the_caller_tenant()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();

        var pending = Flagged(tenantId, Now);
        evaluations.Seed(pending);

        var normal = NotFlagged(tenantId, Now.AddMinutes(1));
        evaluations.Seed(normal);

        var alreadyReviewed = Flagged(tenantId, Now.AddMinutes(2));
        alreadyReviewed.MarkReviewed("Confirmed", Guid.NewGuid(), Now.AddMinutes(3));
        evaluations.Seed(alreadyReviewed);

        var alreadyOverridden = Flagged(tenantId, Now.AddMinutes(4));
        alreadyOverridden.Override(6.0m, 6.0m, 6.0m, 6.0m, "reason", Guid.NewGuid(), Now.AddMinutes(5));
        evaluations.Seed(alreadyOverridden);

        var otherTenant = Flagged(Guid.NewGuid(), Now);
        evaluations.Seed(otherTenant);

        var handler = CreateHandler(evaluations, "Teacher");
        var result = await handler.Handle(new GetWritingReviewQueueQuery(tenantId, null, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(pending.Id.Value, item.EvaluationId);
    }

    [Fact]
    public async Task Non_teacher_caller_is_rejected()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();
        evaluations.Seed(Flagged(tenantId, Now));

        var handler = CreateHandler(evaluations, "Student");
        var result = await handler.Handle(new GetWritingReviewQueueQuery(tenantId, null, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_WRITING_REVIEW_QUEUE_TEACHER_ONLY", result.Error.Code);
    }

    [Fact]
    public async Task Pages_cursor_by_created_at_descending()
    {
        var tenantId = Guid.NewGuid();
        var evaluations = new FakeWritingEvaluationRepository();
        var oldest = Flagged(tenantId, Now);
        var newest = Flagged(tenantId, Now.AddMinutes(10));
        evaluations.Seed(oldest);
        evaluations.Seed(newest);

        var handler = CreateHandler(evaluations, "Teacher");
        var firstPage = await handler.Handle(new GetWritingReviewQueueQuery(tenantId, null, 1), CancellationToken.None);

        Assert.True(firstPage.IsSuccess);
        var firstItem = Assert.Single(firstPage.Value.Items);
        Assert.Equal(newest.Id.Value, firstItem.EvaluationId);
        Assert.True(firstPage.Value.HasMore);

        var secondPage = await handler.Handle(
            new GetWritingReviewQueueQuery(tenantId, firstPage.Value.NextCursor, 1), CancellationToken.None);

        Assert.True(secondPage.IsSuccess);
        var secondItem = Assert.Single(secondPage.Value.Items);
        Assert.Equal(oldest.Id.Value, secondItem.EvaluationId);
        Assert.False(secondPage.Value.HasMore);
    }
}
