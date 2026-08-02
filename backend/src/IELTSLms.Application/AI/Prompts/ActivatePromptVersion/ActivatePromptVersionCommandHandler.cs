using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.ActivatePromptVersion;

public sealed class ActivatePromptVersionCommandHandler(
    IAiPromptRepository prompts, IUnitOfWork unitOfWork, IClock clock, IEnumerable<IPromptActivationValidator> activationValidators)
    : ICommandHandler<ActivatePromptVersionCommand, AiPromptResponse>
{
    public async Task<Result<AiPromptResponse>> Handle(ActivatePromptVersionCommand request, CancellationToken cancellationToken)
    {
        var target = await prompts.GetByIdAsync(AiPromptId.From(request.PromptId), cancellationToken).ConfigureAwait(false);
        if (target is null || target.TenantId != request.TenantId)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptNotFound);
        }

        if (request.IfMatchVersion is { } expected && expected != target.Version)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptVersionMismatch);
        }

        if (!target.IsActive)
        {
            // Calibration gating (Phase 7 Sprint 5): a feature-specific validator, if one is registered,
            // must pass before this version is allowed to go live — a pre-activation check, not a manual
            // step someone can skip.
            var validator = activationValidators.FirstOrDefault(v => v.Feature == target.Feature);
            if (validator is not null)
            {
                var validation = await validator.ValidateAsync(request.TenantId, target, cancellationToken).ConfigureAwait(false);
                if (validation.IsFailure)
                {
                    return Result.Failure<AiPromptResponse>(validation.Error);
                }
            }

            var now = clock.UtcNow;
            var slotVersions = await prompts
                .GetVersionsAsync(request.TenantId, target.Feature, target.Scope, cancellationToken)
                .ConfigureAwait(false);

            var currentlyActive = slotVersions.FirstOrDefault(p => p.IsActive && p.Id != target.Id);
            currentlyActive?.Deactivate(now);
            target.Activate(now);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(AiPromptResponseMapper.ToResponse(target));
    }
}
