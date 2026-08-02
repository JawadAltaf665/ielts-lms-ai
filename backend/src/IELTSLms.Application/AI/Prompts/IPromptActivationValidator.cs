using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts;

/// <summary>
/// Optional pre-activation gate for a specific <see cref="AiPrompt.Feature"/>, run by
/// <see cref="ActivatePromptVersion.ActivatePromptVersionCommandHandler"/> before a candidate version is
/// allowed to go live. Registered as a collection (zero or one validator matches a given activation) so
/// each quality-governed capability owns its own gate without the generic activation handler knowing
/// anything Speaking/Writing-specific (Phase 7 Sprint 5's calibration gate is the first implementation;
/// Sprint 6's Writing Examiner is documented to reuse this same extension point rather than inventing
/// its own).
/// </summary>
public interface IPromptActivationValidator
{
    /// <summary>The <see cref="AiPrompt.Feature"/> this validator gates.</summary>
    string Feature { get; }

    /// <summary>Returns a failure to block activation; the candidate is not yet persisted as active when this runs.</summary>
    Task<Result> ValidateAsync(Guid tenantId, AiPrompt candidate, CancellationToken cancellationToken);
}
