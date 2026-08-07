using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.AI.WritingExaminer;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// Runs a candidate examiner/rubric prompt pair against <see cref="WritingCalibrationSampleSet"/>
/// through the same <see cref="IAiGateway"/>/composition path production uses (minus persistence — no
/// <c>AiResponse</c>/<c>WritingEvaluation</c> rows are written, and no Grammar/Vocabulary agent calls —
/// this harness isolates the Examiner step's scoring behavior against known text, same simplification
/// <see cref="SpeakingCalibrationHarness"/> makes by not re-running transcription), and compares the
/// produced band to each sample's expert band. Statistical tolerance, per Testing Strategy §15 — never a
/// single-sample oracle.
/// </summary>
public sealed class WritingCalibrationHarness(
    IAiProviderRepository providers, IAiGateway gateway, IPromptCompositionService promptComposition)
    : IWritingCalibrationHarness
{
    /// <summary>Mean absolute band error, in bands, tolerated before a candidate prompt fails calibration.</summary>
    public const decimal ToleranceBands = 1.0m;

    public async Task<Result<WritingCalibrationReport>> RunAsync(
        Guid tenantId, AiPrompt examinerPrompt, AiPrompt rubricPrompt, CancellationToken cancellationToken)
    {
        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        if (provider is null)
        {
            return Result.Failure<WritingCalibrationReport>(AiErrors.PromptNotConfigured);
        }

        var absoluteErrors = new List<decimal>();

        foreach (var sample in WritingCalibrationSampleSet.Samples)
        {
            var userContent = JsonSerializer.Serialize(
                new { taskPrompt = sample.TaskPrompt, taskType = sample.TaskType, responseText = sample.ResponseText },
                JsonSerializerOptions.Web);

            var composed = promptComposition.Compose(
                examinerPrompt, new Dictionary<string, string> { ["rubric"] = rubricPrompt.SystemTemplate }, userContent);
            if (composed.IsFailure)
            {
                return Result.Failure<WritingCalibrationReport>(composed.Error);
            }

            var invocation = await gateway.InvokeAsync(
                new AiGatewayRequest(
                    tenantId, AiFeatures.WritingExaminer, examinerPrompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                    composed.Value.UserContent, WritingExaminerSchema.Name, WritingExaminerSchema.Json, 2048),
                cancellationToken).ConfigureAwait(false);
            if (invocation.IsFailure)
            {
                return Result.Failure<WritingCalibrationReport>(invocation.Error);
            }

            WritingExaminerOutput? output;
            try
            {
                output = JsonSerializer.Deserialize<WritingExaminerOutput>(invocation.Value.OutputJson, JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                output = null;
            }

            if (output is null)
            {
                return Result.Failure<WritingCalibrationReport>(AiErrors.InputRejected);
            }

            var producedBand = WritingBandCalculator.ToBand(
                output.TaskResponse, output.CoherenceCohesion, output.LexicalResource, output.GrammaticalRangeAccuracy);
            var expertBand = WritingBandCalculator.ToBand(
                sample.ExpertTaskResponse, sample.ExpertCoherenceCohesion, sample.ExpertLexicalResource,
                sample.ExpertGrammaticalRangeAccuracy);

            absoluteErrors.Add(Math.Abs(producedBand - expertBand));
        }

        var meanAbsoluteError = absoluteErrors.Average();
        var report = new WritingCalibrationReport(absoluteErrors.Count, meanAbsoluteError, meanAbsoluteError <= ToleranceBands);

        return Result.Success(report);
    }
}
