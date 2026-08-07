using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Evaluation;
using IELTSLms.Application.AI.Prompts;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing;

/// <summary>
/// Gates activation of any <c>writing-examiner</c>-feature <see cref="AiPrompt"/> version (either the
/// <see cref="WritingExaminerScopes.Default"/> examiner template or the
/// <see cref="WritingExaminerScopes.Rubric"/> artifact — both change scoring behavior) on
/// <see cref="WritingCalibrationHarness"/>'s tolerance check against the held-out expert-rated set.
/// Mirrors <see cref="Speaking.SpeakingExaminerPromptActivationValidator"/> exactly.
/// </summary>
public sealed class WritingExaminerPromptActivationValidator(IAiPromptRepository prompts, IWritingCalibrationHarness harness)
    : IPromptActivationValidator
{
    public string Feature => AiFeatures.WritingExaminer;

    public async Task<Result> ValidateAsync(Guid tenantId, AiPrompt candidate, CancellationToken cancellationToken)
    {
        var examinerPrompt = candidate.Scope == WritingExaminerScopes.Default
            ? candidate
            : await prompts
                .GetActiveForFeatureAndScopeAsync(tenantId, AiFeatures.WritingExaminer, WritingExaminerScopes.Default, cancellationToken)
                .ConfigureAwait(false);

        var rubricPrompt = candidate.Scope == WritingExaminerScopes.Rubric
            ? candidate
            : await prompts
                .GetActiveForFeatureAndScopeAsync(tenantId, AiFeatures.WritingExaminer, WritingExaminerScopes.Rubric, cancellationToken)
                .ConfigureAwait(false);

        // ponytail: if the counterpart half (examiner template or rubric) has no active version yet —
        // only possible on the very first-ever activation of either half, before both exist — there is
        // nothing to compose and calibrate against, so activation is allowed uncalibrated this once.
        // Every subsequent activation, once both halves exist, is gated. Same bootstrap gap as
        // SpeakingExaminerPromptActivationValidator; same upgrade path if it ever matters in practice.
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
