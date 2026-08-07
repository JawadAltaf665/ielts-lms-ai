using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Evaluation;
using IELTSLms.Application.AI.Prompts;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking;

/// <summary>
/// Gates activation of any <c>speaking-examiner</c>-feature <see cref="AiPrompt"/> version (either the
/// <see cref="SpeakingExaminerScopes.Default"/> examiner template or the
/// <see cref="SpeakingExaminerScopes.Rubric"/> artifact — both change scoring behavior) on
/// <see cref="SpeakingCalibrationHarness"/>'s tolerance check against the held-out expert-rated set.
/// </summary>
public sealed class SpeakingExaminerPromptActivationValidator(IAiPromptRepository prompts, ISpeakingCalibrationHarness harness)
    : IPromptActivationValidator
{
    public string Feature => AiFeatures.SpeakingExaminer;

    public async Task<Result> ValidateAsync(Guid tenantId, AiPrompt candidate, CancellationToken cancellationToken)
    {
        var examinerPrompt = candidate.Scope == SpeakingExaminerScopes.Default
            ? candidate
            : await prompts
                .GetActiveForFeatureAndScopeAsync(tenantId, AiFeatures.SpeakingExaminer, SpeakingExaminerScopes.Default, cancellationToken)
                .ConfigureAwait(false);

        var rubricPrompt = candidate.Scope == SpeakingExaminerScopes.Rubric
            ? candidate
            : await prompts
                .GetActiveForFeatureAndScopeAsync(tenantId, AiFeatures.SpeakingExaminer, SpeakingExaminerScopes.Rubric, cancellationToken)
                .ConfigureAwait(false);

        // ponytail: if the counterpart half (examiner template or rubric) has no active version yet —
        // only possible on the very first-ever activation of either half, before both exist — there is
        // nothing to compose and calibrate against, so activation is allowed uncalibrated this once.
        // Every subsequent activation, once both halves exist, is gated. Upgrade to a required
        // "activate both together" flow if this bootstrap gap ever matters in practice.
        if (examinerPrompt is null || rubricPrompt is null)
        {
            return Result.Success();
        }

        var report = await harness.RunAsync(tenantId, examinerPrompt, rubricPrompt, cancellationToken).ConfigureAwait(false);
        if (report.IsFailure)
        {
            return Result.Failure(report.Error);
        }

        return report.Value.WithinTolerance ? Result.Success() : Result.Failure(AiErrors.PromptCalibrationFailed);
    }
}
