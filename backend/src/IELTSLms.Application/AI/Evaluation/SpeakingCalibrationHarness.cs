using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.AI.SpeakingExaminer;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// Runs a candidate examiner/rubric prompt pair against <see cref="SpeakingCalibrationSampleSet"/>
/// through the same <see cref="IAiGateway"/>/composition path production uses (minus persistence — no
/// <c>AiResponse</c>/<c>SpeakingEvaluation</c> rows are written), and compares the produced band to each
/// sample's expert band. Statistical tolerance, per Testing Strategy §15 — never a single-sample oracle.
/// </summary>
public sealed class SpeakingCalibrationHarness(
    IAiProviderRepository providers, IAiGateway gateway, IPromptCompositionService promptComposition)
    : ISpeakingCalibrationHarness
{
    /// <summary>Mean absolute band error, in bands, tolerated before a candidate prompt fails calibration.</summary>
    public const decimal ToleranceBands = 1.0m;

    public async Task<Result<SpeakingCalibrationReport>> RunAsync(
        Guid tenantId, AiPrompt examinerPrompt, AiPrompt rubricPrompt, CancellationToken cancellationToken)
    {
        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        if (provider is null)
        {
            return Result.Failure<SpeakingCalibrationReport>(AiErrors.PromptNotConfigured);
        }

        var absoluteErrors = new List<decimal>();

        foreach (var sample in SpeakingCalibrationSampleSet.Samples)
        {
            var userContent = JsonSerializer.Serialize(
                new { taskPrompt = sample.TaskPrompt, part = sample.Part, transcript = sample.Transcript }, JsonSerializerOptions.Web);

            var composed = promptComposition.Compose(
                examinerPrompt, new Dictionary<string, string> { ["rubric"] = rubricPrompt.SystemTemplate }, userContent);
            if (composed.IsFailure)
            {
                return Result.Failure<SpeakingCalibrationReport>(composed.Error);
            }

            var invocation = await gateway.InvokeAsync(
                new AiGatewayRequest(
                    tenantId, AiFeatures.SpeakingExaminer, examinerPrompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                    composed.Value.UserContent, SpeakingExaminerSchema.Name, SpeakingExaminerSchema.Json, 2048),
                cancellationToken).ConfigureAwait(false);
            if (invocation.IsFailure)
            {
                return Result.Failure<SpeakingCalibrationReport>(invocation.Error);
            }

            SpeakingExaminerOutput? output;
            try
            {
                output = JsonSerializer.Deserialize<SpeakingExaminerOutput>(invocation.Value.OutputJson, JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                output = null;
            }

            if (output is null)
            {
                return Result.Failure<SpeakingCalibrationReport>(AiErrors.InputRejected);
            }

            var producedBand = SpeakingBandCalculator.ToBand(
                output.FluencyCoherence, output.LexicalResource, output.GrammaticalRangeAccuracy, output.Pronunciation);
            var expertBand = SpeakingBandCalculator.ToBand(
                sample.ExpertFluencyCoherence, sample.ExpertLexicalResource, sample.ExpertGrammaticalRangeAccuracy,
                sample.ExpertPronunciation);

            absoluteErrors.Add(Math.Abs(producedBand - expertBand));
        }

        var meanAbsoluteError = absoluteErrors.Average();
        var report = new SpeakingCalibrationReport(absoluteErrors.Count, meanAbsoluteError, meanAbsoluteError <= ToleranceBands);

        return Result.Success(report);
    }
}
