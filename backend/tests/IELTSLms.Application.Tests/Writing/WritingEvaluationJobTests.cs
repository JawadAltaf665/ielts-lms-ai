using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.SimilarityCheck;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.Writing;

/// <summary>
/// Tier-1 tests for the Writing Evaluation Workflow's simplified async-pipeline implementation (ADR
/// 0001): Grammar Agent + Vocabulary Agent in parallel, then Writing Examiner, mirroring
/// <c>GrammarCoachingJobTests</c>'s fixture shape.
/// </summary>
public sealed class WritingEvaluationJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private const string ValidGrammarOutput = """{"corrections":[]}""";
    private const string ValidVocabOutput = """{"issues":[],"lexicalVarietyNote":"Reasonable range."}""";
    private const string ValidExaminerOutput =
        """{"taskResponse":7,"coherenceCohesion":6.5,"lexicalResource":7,"grammaticalRangeAccuracy":6.5,"confidence":0.9,"feedbackText":"Good essay.","rationale":"Solid structure."}""";

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiProviderRepository Providers, FakeAiPromptRepository Prompts,
        FakeAiResponseRepository Responses, FakeWritingSubmissionRepository Submissions, FakeWritingTaskRepository Tasks,
        FakeWritingEvaluationRepository Evaluations, FakeUnitOfWork UnitOfWork, FakeWritingAiGateway Gateway,
        FakeTenantContextAccessor TenantContextAccessor, WritingEvaluationJob Job, Guid TenantId, AiOperation Operation,
        WritingSubmission Submission);

    private static Fixture CreateFixture(bool seedPrompts = true, bool seedRubricPrompt = true)
    {
        var tenantId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var submissions = new FakeWritingSubmissionRepository();
        var tasks = new FakeWritingTaskRepository();
        var evaluations = new FakeWritingEvaluationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeWritingAiGateway();
        var tenantContextAccessor = new FakeTenantContextAccessor();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        if (seedPrompts)
        {
            prompts.Seed(Active(AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), null, provider.Id, "grammar-coach", 1, "default", "sys", Now).Value));
            prompts.Seed(Active(AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), null, provider.Id, "vocabulary-analysis", 1, "default", "sys", Now).Value));
            prompts.Seed(Active(AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), null, provider.Id, "writing-examiner", 1, "default", "sys {{rubric}}", Now).Value));
        }

        if (seedRubricPrompt)
        {
            prompts.Seed(Active(AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), null, provider.Id, "writing-examiner", 1, "rubric", "score each criterion", Now).Value));
        }

        var task = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId, "Task 2", WritingTaskType.Task2, ReadingTrack.Academic, "Discuss.", null, 3, 100,
            null, Now).Value;
        tasks.Seed(task);

        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), task.Id, "This essay discusses the topic.", Now).Value;
        submission.Submit(6, Now);
        submissions.Seed(submission);

        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), tenantId, submission.UserId, "writing-evaluation", submission.ResponseText, Now);
        operations.Seed(operation);

        var job = new WritingEvaluationJob(
            operations, providers, prompts, responses, submissions, tasks, evaluations, unitOfWork, gateway,
            new SimilarityCheckProvider(submissions), new PromptCompositionService(), tenantContextAccessor,
            new FakeClock(Now.AddSeconds(5)), new FakeGuidProvider(), NullLogger<WritingEvaluationJob>.Instance);

        return new Fixture(
            operations, providers, prompts, responses, submissions, tasks, evaluations, unitOfWork, gateway, tenantContextAccessor, job,
            tenantId, operation, submission);
    }

    private static Result<AiGatewayInvocationResult> Success(string outputJson) =>
        Result.Success(new AiGatewayInvocationResult(outputJson, "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false));

    [Fact]
    public async Task Full_success_creates_an_evaluation_and_succeeds_the_operation()
    {
        var fixture = CreateFixture();
        fixture.Gateway.SetResult("grammar-coach", Success(ValidGrammarOutput));
        fixture.Gateway.SetResult("vocabulary-analysis", Success(ValidVocabOutput));
        fixture.Gateway.SetResult("writing-examiner", Success(ValidExaminerOutput));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        Assert.Equal(WritingSubmissionStatus.Evaluated, fixture.Submission.Status);
        var evaluation = Assert.Single(fixture.Evaluations.Evaluations);
        Assert.Equal(7.0m, evaluation.TaskResponse);
        Assert.Equal(6.5m, evaluation.CoherenceCohesion);
        Assert.False(evaluation.RequiresHumanReview);
        Assert.Equal(3, fixture.Responses.Responses.Count);
        Assert.Equal(fixture.TenantId, fixture.TenantContextAccessor.TenantId);
    }

    [Fact]
    public async Task Low_confidence_flags_the_evaluation_for_human_review_rather_than_silently_accepting_it()
    {
        var fixture = CreateFixture();
        const string lowConfidenceExaminerOutput =
            """{"taskResponse":7,"coherenceCohesion":6.5,"lexicalResource":7,"grammaticalRangeAccuracy":6.5,"confidence":0.4,"feedbackText":"Good essay.","rationale":"Uncertain."}""";
        fixture.Gateway.SetResult("grammar-coach", Success(ValidGrammarOutput));
        fixture.Gateway.SetResult("vocabulary-analysis", Success(ValidVocabOutput));
        fixture.Gateway.SetResult("writing-examiner", Success(lowConfidenceExaminerOutput));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        var evaluation = Assert.Single(fixture.Evaluations.Evaluations);
        Assert.True(evaluation.RequiresHumanReview);
    }

    [Fact]
    public async Task A_near_duplicate_prior_submission_for_the_same_task_flags_the_evaluation_via_similarity()
    {
        var fixture = CreateFixture();
        var priorSubmission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), fixture.TenantId, Guid.NewGuid(), fixture.Submission.WritingTaskId,
            fixture.Submission.ResponseText, Now).Value;
        priorSubmission.Submit(6, Now);
        fixture.Submissions.Seed(priorSubmission);

        fixture.Gateway.SetResult("grammar-coach", Success(ValidGrammarOutput));
        fixture.Gateway.SetResult("vocabulary-analysis", Success(ValidVocabOutput));
        fixture.Gateway.SetResult("writing-examiner", Success(ValidExaminerOutput));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        var evaluation = Assert.Single(fixture.Evaluations.Evaluations);
        Assert.Equal(1.0m, evaluation.SimilarityScore);
        Assert.True(evaluation.RequiresHumanReview);
    }

    [Fact]
    public async Task Grammar_agent_failure_fails_both_the_operation_and_the_submission()
    {
        var fixture = CreateFixture();
        fixture.Gateway.SetResult(
            "grammar-coach", Result.Failure<AiGatewayInvocationResult>(Error.ServiceUnavailable("DEPENDENCY_AI_UNAVAILABLE", "down")));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal(WritingSubmissionStatus.Failed, fixture.Submission.Status);
        Assert.Empty(fixture.Evaluations.Evaluations);
    }

    [Fact]
    public async Task Malformed_examiner_output_fails_the_workflow_without_a_crash()
    {
        var fixture = CreateFixture();
        fixture.Gateway.SetResult("grammar-coach", Success(ValidGrammarOutput));
        fixture.Gateway.SetResult("vocabulary-analysis", Success(ValidVocabOutput));
        fixture.Gateway.SetResult("writing-examiner", Success("not json"));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal(WritingSubmissionStatus.Failed, fixture.Submission.Status);
        Assert.Empty(fixture.Evaluations.Evaluations);
        Assert.Equal(3, fixture.Responses.Responses.Count);
    }

    [Fact]
    public async Task Missing_prompt_configuration_fails_gracefully_without_calling_the_gateway()
    {
        var fixture = CreateFixture(seedPrompts: false);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", fixture.Operation.ErrorCode);
        Assert.Equal(WritingSubmissionStatus.Failed, fixture.Submission.Status);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Missing_rubric_configuration_fails_gracefully_without_calling_the_gateway()
    {
        var fixture = CreateFixture(seedRubricPrompt: false);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", fixture.Operation.ErrorCode);
        Assert.Equal(WritingSubmissionStatus.Failed, fixture.Submission.Status);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Submission.Id.Value, CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    private static AiPrompt Active(AiPrompt prompt)
    {
        prompt.Activate(Now);
        return prompt;
    }
}
