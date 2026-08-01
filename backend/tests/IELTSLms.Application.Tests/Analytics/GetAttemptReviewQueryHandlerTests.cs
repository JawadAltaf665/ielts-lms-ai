using IELTSLms.Application.Analytics.Queries.GetAttemptReview;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.Writing;
using FakeCurrentUserService = IELTSLms.Application.Tests.Writing.Fakes.FakeCurrentUserService;

namespace IELTSLms.Application.Tests.Analytics;

public sealed class GetAttemptReviewQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static GetAttemptReviewQueryHandler CreateHandler(
        FakeWritingSubmissionRepository writingSubmissions, FakeWritingEvaluationRepository writingEvaluations,
        FakeCurrentUserService currentUser) =>
        new(writingSubmissions, writingEvaluations, new FakeSpeakingAttemptRepository(), new FakeSpeakingEvaluationRepository(),
            new FakeReadingAttemptRepository(), new FakeListeningAttemptRepository(), new FakeGrammarAttemptRepository(),
            new FakeVocabularyProgressRepository(), new FakeMockExamResultRepository(), currentUser);

    [Fact]
    public async Task Owner_can_review_their_own_writing_submission()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var writingSubmissions = new FakeWritingSubmissionRepository();
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "response text", Now).Value;
        submission.Submit(2, Now);
        writingSubmissions.Seed(submission);
        var handler = CreateHandler(writingSubmissions, new FakeWritingEvaluationRepository(), new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetAttemptReviewQuery(tenantId, userId, "writing", submission.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("writing", result.Value.Type);
        Assert.Equal("Submitted", result.Value.Status);
    }

    [Fact]
    public async Task A_non_owner_who_is_not_a_teacher_gets_not_found()
    {
        var tenantId = Guid.NewGuid();
        var writingSubmissions = new FakeWritingSubmissionRepository();
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), WritingTaskId.From(Guid.NewGuid()), "response text", Now).Value;
        submission.Submit(2, Now);
        writingSubmissions.Seed(submission);
        var handler = CreateHandler(writingSubmissions, new FakeWritingEvaluationRepository(), new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetAttemptReviewQuery(tenantId, Guid.NewGuid(), "writing", submission.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task An_unknown_attempt_type_is_rejected()
    {
        var handler = CreateHandler(
            new FakeWritingSubmissionRepository(), new FakeWritingEvaluationRepository(), new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetAttemptReviewQuery(Guid.NewGuid(), Guid.NewGuid(), "not-a-real-type", Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_ATTEMPT_TYPE_INVALID", result.Error.Code);
    }
}
