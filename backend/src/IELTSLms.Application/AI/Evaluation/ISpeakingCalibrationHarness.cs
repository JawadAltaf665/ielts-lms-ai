using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// Scores the held-out expert-rated Speaking calibration set (Testing Strategy §15, AI Architecture
/// §11.3) with a candidate <c>speaking-examiner</c>/<c>rubric</c> prompt pair, offline — never on the
/// production submit→evaluate request path. The result gates prompt-version activation
/// (<see cref="Prompts.IPromptActivationValidator"/>); Sprint 6 (Writing Examiner) is documented to
/// reuse this same shape rather than invent its own eval harness.
/// </summary>
public interface ISpeakingCalibrationHarness
{
    Task<Result<SpeakingCalibrationReport>> RunAsync(
        Guid tenantId, AiPrompt examinerPrompt, AiPrompt rubricPrompt, CancellationToken cancellationToken);
}

/// <summary>
/// <paramref name="WithinTolerance"/> is a statistical pass/fail on <paramref name="MeanAbsoluteBandError"/>
/// against <see cref="SpeakingCalibrationHarness.ToleranceBands"/> — never a single-sample exact match,
/// per Testing Strategy §15's "AI tests are statistical, not binary" principle.
/// </summary>
public sealed record SpeakingCalibrationReport(int SampleCount, decimal MeanAbsoluteBandError, bool WithinTolerance);
