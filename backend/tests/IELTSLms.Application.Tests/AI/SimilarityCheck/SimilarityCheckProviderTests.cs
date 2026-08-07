using IELTSLms.Application.AI.SimilarityCheck;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.AI.SimilarityCheck;

public sealed class SimilarityCheckProviderTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private const string EssayText =
        "Some governments believe that public transport should be free for all citizens in order to " +
        "reduce traffic congestion and pollution in major cities across the country every single year.";

    private static WritingTask CreateTask(Guid tenantId) =>
        WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId, "Task 2", WritingTaskType.Task2, ReadingTrack.Academic, "Discuss.", null, 3, 500,
            null, Now).Value;

    private static WritingSubmission Submitted(Guid tenantId, WritingTaskId taskId, string responseText)
    {
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), taskId, responseText, Now).Value;
        submission.Submit(responseText.Split(' ').Length, Now);
        return submission;
    }

    [Fact]
    public async Task An_identical_prior_submission_for_the_same_task_scores_a_perfect_match()
    {
        var tenantId = Guid.NewGuid();
        var task = CreateTask(tenantId);
        var submissions = new FakeWritingSubmissionRepository();
        var prior = Submitted(tenantId, task.Id, EssayText);
        submissions.Seed(prior);

        var provider = new SimilarityCheckProvider(submissions);
        var candidateId = WritingSubmissionId.From(Guid.NewGuid());

        var result = await provider.CheckAsync(tenantId, task.Id, candidateId, EssayText, CancellationToken.None);

        Assert.Equal(1.0m, result.Score);
        Assert.Equal(prior.Id.Value, result.MatchedSubmissionId);
    }

    [Fact]
    public async Task An_original_response_with_no_comparable_prior_text_scores_zero()
    {
        var tenantId = Guid.NewGuid();
        var task = CreateTask(tenantId);
        var submissions = new FakeWritingSubmissionRepository();
        submissions.Seed(Submitted(
            tenantId, task.Id,
            "Nutrition education in primary schools has become a widely debated topic among policymakers " +
            "and parents alike, particularly given rising childhood obesity rates worldwide."));

        var provider = new SimilarityCheckProvider(submissions);
        var candidateId = WritingSubmissionId.From(Guid.NewGuid());

        var result = await provider.CheckAsync(tenantId, task.Id, candidateId, EssayText, CancellationToken.None);

        Assert.Equal(0m, result.Score);
        Assert.Null(result.MatchedSubmissionId);
    }

    [Fact]
    public async Task An_empty_corpus_scores_zero_without_a_match()
    {
        var tenantId = Guid.NewGuid();
        var task = CreateTask(tenantId);
        var submissions = new FakeWritingSubmissionRepository();
        var provider = new SimilarityCheckProvider(submissions);

        var result = await provider.CheckAsync(
            tenantId, task.Id, WritingSubmissionId.From(Guid.NewGuid()), EssayText, CancellationToken.None);

        Assert.Equal(0m, result.Score);
        Assert.Null(result.MatchedSubmissionId);
    }

    [Fact]
    public async Task A_matching_submission_for_a_different_task_is_not_compared_against()
    {
        var tenantId = Guid.NewGuid();
        var task = CreateTask(tenantId);
        var otherTask = CreateTask(tenantId);
        var submissions = new FakeWritingSubmissionRepository();
        submissions.Seed(Submitted(tenantId, otherTask.Id, EssayText));

        var provider = new SimilarityCheckProvider(submissions);
        var result = await provider.CheckAsync(
            tenantId, task.Id, WritingSubmissionId.From(Guid.NewGuid()), EssayText, CancellationToken.None);

        Assert.Equal(0m, result.Score);
    }
}
