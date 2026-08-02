using IELTSLms.Application.AI.Evaluation;
using IELTSLms.Application.Speaking;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class SpeakingExaminerPromptActivationValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private sealed class FakeHarness : ISpeakingCalibrationHarness
    {
        public Result<SpeakingCalibrationReport>? NextResult { get; set; }

        public int RunCallCount { get; private set; }

        public Task<Result<SpeakingCalibrationReport>> RunAsync(
            Guid tenantId, AiPrompt examinerPrompt, AiPrompt rubricPrompt, CancellationToken cancellationToken)
        {
            RunCallCount++;
            return Task.FromResult(NextResult ?? Result.Success(new SpeakingCalibrationReport(5, 0m, WithinTolerance: true)));
        }
    }

    private static AiPrompt CreatePrompt(Guid? tenantId, string scope, int version) =>
        AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), tenantId, null, "speaking-examiner", version, scope, "template", Now).Value;

    [Fact]
    public async Task Activating_a_default_scope_candidate_against_an_active_rubric_runs_the_harness()
    {
        var prompts = new FakeAiPromptRepository();
        var activeRubric = CreatePrompt(_tenantId, "rubric", 1);
        activeRubric.Activate(Now);
        prompts.Seed(activeRubric);

        var candidate = CreatePrompt(_tenantId, "default", 2);
        var harness = new FakeHarness { NextResult = Result.Success(new SpeakingCalibrationReport(5, 0.5m, WithinTolerance: true)) };
        var validator = new SpeakingExaminerPromptActivationValidator(prompts, harness);

        var result = await validator.ValidateAsync(_tenantId, candidate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, harness.RunCallCount);
    }

    [Fact]
    public async Task A_failing_calibration_report_blocks_activation()
    {
        var prompts = new FakeAiPromptRepository();
        var activeRubric = CreatePrompt(_tenantId, "rubric", 1);
        activeRubric.Activate(Now);
        prompts.Seed(activeRubric);

        var candidate = CreatePrompt(_tenantId, "default", 2);
        var harness = new FakeHarness { NextResult = Result.Success(new SpeakingCalibrationReport(5, 2.0m, WithinTolerance: false)) };
        var validator = new SpeakingExaminerPromptActivationValidator(prompts, harness);

        var result = await validator.ValidateAsync(_tenantId, candidate, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_AI_PROMPT_CALIBRATION_FAILED", result.Error.Code);
    }

    [Fact]
    public async Task Bootstrap_the_very_first_activation_with_no_active_counterpart_is_allowed_uncalibrated()
    {
        var prompts = new FakeAiPromptRepository();
        var candidate = CreatePrompt(_tenantId, "default", 1);
        var harness = new FakeHarness();
        var validator = new SpeakingExaminerPromptActivationValidator(prompts, harness);

        var result = await validator.ValidateAsync(_tenantId, candidate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, harness.RunCallCount);
    }

    [Fact]
    public async Task Activating_a_rubric_scope_candidate_against_an_active_examiner_template_runs_the_harness()
    {
        var prompts = new FakeAiPromptRepository();
        var activeExaminer = CreatePrompt(_tenantId, "default", 1);
        activeExaminer.Activate(Now);
        prompts.Seed(activeExaminer);

        var candidate = CreatePrompt(_tenantId, "rubric", 2);
        var harness = new FakeHarness();
        var validator = new SpeakingExaminerPromptActivationValidator(prompts, harness);

        var result = await validator.ValidateAsync(_tenantId, candidate, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, harness.RunCallCount);
    }
}
