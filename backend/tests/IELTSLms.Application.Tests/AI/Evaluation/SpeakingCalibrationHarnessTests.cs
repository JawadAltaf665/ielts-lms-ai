using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Evaluation;
using IELTSLms.Application.AI.SpeakingExaminer;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Evaluation;

public sealed class SpeakingCalibrationHarnessTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private static (AiPrompt Examiner, AiPrompt Rubric) CreatePromptPair()
    {
        var examiner = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "speaking-examiner", 2, "default", "sys {{rubric}}", Now).Value;
        var rubric = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, null, "speaking-examiner", 1, "rubric", "score each criterion", Now).Value;
        return (examiner, rubric);
    }

    private static string OutputFor(decimal fc, decimal lr, decimal gra, decimal pr) =>
        JsonSerializer.Serialize(new SpeakingExaminerOutput(fc, lr, gra, pr, 0.9m, "feedback"), JsonSerializerOptions.Web);

    [Fact]
    public async Task A_candidate_that_matches_the_expert_scores_exactly_is_within_tolerance()
    {
        var providers = new FakeAiProviderRepository();
        var provider = AiProvider.Create(AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://x", Now).Value;
        providers.Seed(provider);

        var gateway = new FakeAiGateway();
        foreach (var sample in SpeakingCalibrationSampleSet.Samples)
        {
            var output = OutputFor(
                sample.ExpertFluencyCoherence, sample.ExpertLexicalResource, sample.ExpertGrammaticalRangeAccuracy,
                sample.ExpertPronunciation);
            gateway.ResultQueue.Enqueue(Result.Success(
                new AiGatewayInvocationResult(output, "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false)));
        }

        var harness = new SpeakingCalibrationHarness(providers, gateway, new PromptCompositionService());
        var (examiner, rubric) = CreatePromptPair();

        var result = await harness.RunAsync(_tenantId, examiner, rubric, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SpeakingCalibrationSampleSet.Samples.Count, result.Value.SampleCount);
        Assert.Equal(0m, result.Value.MeanAbsoluteBandError);
        Assert.True(result.Value.WithinTolerance);
    }

    [Fact]
    public async Task A_candidate_that_scores_everything_the_same_regardless_of_input_fails_tolerance()
    {
        var providers = new FakeAiProviderRepository();
        var provider = AiProvider.Create(AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://x", Now).Value;
        providers.Seed(provider);

        var gateway = new FakeAiGateway
        {
            NextResult = Result.Success(new AiGatewayInvocationResult(
                OutputFor(7.0m, 6.5m, 7.0m, 6.5m), "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false)),
        };

        var harness = new SpeakingCalibrationHarness(providers, gateway, new PromptCompositionService());
        var (examiner, rubric) = CreatePromptPair();

        var result = await harness.RunAsync(_tenantId, examiner, rubric, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.WithinTolerance);
        Assert.True(result.Value.MeanAbsoluteBandError > SpeakingCalibrationHarness.ToleranceBands);
    }

    [Fact]
    public async Task No_active_provider_fails_before_calling_the_gateway()
    {
        var providers = new FakeAiProviderRepository();
        var gateway = new FakeAiGateway();
        var harness = new SpeakingCalibrationHarness(providers, gateway, new PromptCompositionService());
        var (examiner, rubric) = CreatePromptPair();

        var result = await harness.RunAsync(_tenantId, examiner, rubric, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", result.Error.Code);
        Assert.Equal(0, gateway.InvokeCallCount);
    }
}
