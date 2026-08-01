using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.Speaking;

/// <summary>
/// Tier-1 tests for the Speaking Evaluation Workflow: transcribe (STT) then a single Speaking Examiner
/// call, mirroring <c>WritingEvaluationJobTests</c>'s fixture shape. Unlike Writing, this job makes only
/// one AiGateway call, so the shared single-result <see cref="FakeAiGateway"/> is enough (no per-feature
/// dispatch needed).
/// </summary>
public sealed class SpeakingEvaluationJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private const string ValidExaminerOutput =
        """{"fluencyCoherence":7,"lexicalResource":6.5,"grammaticalRangeAccuracy":7,"pronunciation":6.5,"confidence":0.9,"feedbackText":"Good fluency."}""";

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiProviderRepository Providers, FakeAiPromptRepository Prompts,
        FakeAiResponseRepository Responses, FakeSpeakingAttemptRepository Attempts, FakeSpeakingTaskRepository Tasks,
        FakeSpeakingEvaluationRepository Evaluations, FakeFileAssetRepository Files, FakeFileStorage FileStorage,
        FakeSpeechToTextProvider SpeechToText, FakeUnitOfWork UnitOfWork, FakeAiGateway Gateway,
        FakeTenantContextAccessor TenantContextAccessor, SpeakingEvaluationJob Job, Guid TenantId, AiOperation Operation,
        SpeakingAttempt Attempt);

    private static Fixture CreateFixture(bool seedPrompts = true)
    {
        var tenantId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var attempts = new FakeSpeakingAttemptRepository();
        var tasks = new FakeSpeakingTaskRepository();
        var evaluations = new FakeSpeakingEvaluationRepository();
        var files = new FakeFileAssetRepository();
        var clock = new FakeClock(Now.AddSeconds(5));
        var fileStorage = new FakeFileStorage(clock);
        var speechToText = new FakeSpeechToTextProvider();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeAiGateway();
        var tenantContextAccessor = new FakeTenantContextAccessor();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        if (seedPrompts)
        {
            var prompt = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), null, provider.Id, "speaking-examiner", 1, "default", "sys", Now).Value;
            prompt.Activate(Now);
            prompts.Seed(prompt);
        }

        var task = SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), tenantId, "Part 2", SpeakingPart.Part2, "Describe a place.", null, Now).Value;
        tasks.Seed(task);

        var file = FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), "speaking-attempt/recording.mp3", "recording.mp3", "audio/mpeg",
            1024, ["audio/mpeg"], 50 * 1024 * 1024, Now).Value;
        files.Seed(file);

        var attempt = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), task.Id, file.Id, 45, Now).Value;
        attempt.Submit(Now);
        attempts.Seed(attempt);

        var operation = AiOperation.Enqueue(AiOperationId.From(Guid.NewGuid()), tenantId, attempt.UserId, "speaking-evaluation", "", Now);
        operations.Seed(operation);

        var job = new SpeakingEvaluationJob(
            operations, providers, prompts, responses, attempts, tasks, evaluations, files, fileStorage, speechToText, unitOfWork, gateway,
            tenantContextAccessor, clock, new FakeGuidProvider(), NullLogger<SpeakingEvaluationJob>.Instance);

        return new Fixture(
            operations, providers, prompts, responses, attempts, tasks, evaluations, files, fileStorage, speechToText, unitOfWork, gateway,
            tenantContextAccessor, job, tenantId, operation, attempt);
    }

    private static Result<AiGatewayInvocationResult> Success(string outputJson) =>
        Result.Success(new AiGatewayInvocationResult(outputJson, "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false));

    [Fact]
    public async Task Full_success_transcribes_creates_an_evaluation_and_succeeds_the_operation()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Success(ValidExaminerOutput);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        Assert.Equal(SpeakingAttemptStatus.Evaluated, fixture.Attempt.Status);
        Assert.Equal("This is what the learner said.", fixture.Attempt.TranscriptText);
        var evaluation = Assert.Single(fixture.Evaluations.Evaluations);
        Assert.Equal(7.0m, evaluation.FluencyCoherence);
        Assert.Equal(6.5m, evaluation.LexicalResource);
        Assert.False(evaluation.RequiresHumanReview);
        Assert.Single(fixture.Responses.Responses);
        Assert.Equal(1, fixture.SpeechToText.TranscribeCallCount);
        Assert.Equal(fixture.TenantId, fixture.TenantContextAccessor.TenantId);
    }

    [Fact]
    public async Task Low_confidence_flags_the_evaluation_for_human_review_rather_than_silently_accepting_it()
    {
        var fixture = CreateFixture();
        const string lowConfidenceExaminerOutput =
            """{"fluencyCoherence":7,"lexicalResource":6.5,"grammaticalRangeAccuracy":7,"pronunciation":6.5,"confidence":0.4,"feedbackText":"Uncertain."}""";
        fixture.Gateway.NextResult = Success(lowConfidenceExaminerOutput);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        var evaluation = Assert.Single(fixture.Evaluations.Evaluations);
        Assert.True(evaluation.RequiresHumanReview);
    }

    [Fact]
    public async Task STT_provider_failure_fails_the_operation_with_service_unavailable_not_a_silent_empty_transcript()
    {
        var fixture = CreateFixture();
        fixture.SpeechToText.NextResult = Result.Failure<SpeechToTextResult>(
            Error.ServiceUnavailable("SERVICE_UNAVAILABLE_STT_PROVIDER", "down"));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("SERVICE_UNAVAILABLE_STT_PROVIDER", fixture.Operation.ErrorCode);
        Assert.Equal(SpeakingAttemptStatus.Failed, fixture.Attempt.Status);
        Assert.Null(fixture.Attempt.TranscriptText);
        Assert.Empty(fixture.Evaluations.Evaluations);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Malformed_examiner_output_fails_the_workflow_without_a_crash()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Success("not json");

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal(SpeakingAttemptStatus.Failed, fixture.Attempt.Status);
        Assert.Empty(fixture.Evaluations.Evaluations);
        Assert.Single(fixture.Responses.Responses);
    }

    [Fact]
    public async Task Missing_prompt_configuration_fails_gracefully_without_calling_STT_or_the_gateway()
    {
        var fixture = CreateFixture(seedPrompts: false);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", fixture.Operation.ErrorCode);
        Assert.Equal(SpeakingAttemptStatus.Failed, fixture.Attempt.Status);
        Assert.Equal(0, fixture.SpeechToText.TranscribeCallCount);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(0, fixture.SpeechToText.TranscribeCallCount);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
