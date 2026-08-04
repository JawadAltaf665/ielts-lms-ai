using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// Scores the held-out expert-rated Writing calibration set (Testing Strategy §15, AI Architecture
/// §11.3) with a candidate <c>writing-examiner</c>/<c>rubric</c> prompt pair, offline — never on the
/// production submit→evaluate request path. The result gates prompt-version activation
/// (<see cref="Prompts.IPromptActivationValidator"/>). Mirrors <see cref="ISpeakingCalibrationHarness"/>'s
/// shape exactly, per Sprint 5's own guidance to reuse rather than invent a parallel one.
/// </summary>
public interface IWritingCalibrationHarness
{
    Task<Result<WritingCalibrationReport>> RunAsync(
        Guid tenantId, AiPrompt examinerPrompt, AiPrompt rubricPrompt, CancellationToken cancellationToken);
}

/// <summary>
/// <paramref name="WithinTolerance"/> is a statistical pass/fail on <paramref name="MeanAbsoluteBandError"/>
/// against <see cref="WritingCalibrationHarness.ToleranceBands"/> — never a single-sample exact match,
/// per Testing Strategy §15's "AI tests are statistical, not binary" principle.
/// </summary>
public sealed record WritingCalibrationReport(int SampleCount, decimal MeanAbsoluteBandError, bool WithinTolerance);
