using System.Text.Json;
using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.RollbackPromptVersion;

public sealed class RollbackPromptVersionCommandHandler(
    IAiPromptRepository prompts, IUnitOfWork unitOfWork, IClock clock,
    IAuditLogWriter auditLogWriter, ICorrelationIdProvider correlationIdProvider)
    : ICommandHandler<RollbackPromptVersionCommand, AiPromptResponse>
{
    public async Task<Result<AiPromptResponse>> Handle(RollbackPromptVersionCommand request, CancellationToken cancellationToken)
    {
        var current = await prompts.GetByIdAsync(AiPromptId.From(request.PromptId), cancellationToken).ConfigureAwait(false);
        if (current is null || current.TenantId != request.TenantId)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptNotFound);
        }

        if (!current.IsActive)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptNotActive);
        }

        var slotVersions = await prompts
            .GetVersionsAsync(request.TenantId, current.Feature, current.Scope, cancellationToken)
            .ConfigureAwait(false);

        // The only way a version in this slot ever becomes inactive is by being superseded by another
        // activation (see ActivatePromptVersionCommandHandler) — so the most recently deactivated
        // version here is, by construction, whatever was active immediately before the current one.
        var previous = slotVersions
            .Where(p => p.Id != current.Id && !p.IsActive)
            .OrderByDescending(p => p.UpdatedAtUtc)
            .ThenByDescending(p => p.PromptVersion)
            .FirstOrDefault();

        if (previous is null)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptNoPriorVersion);
        }

        var now = clock.UtcNow;
        var fromVersion = current.PromptVersion;
        current.Deactivate(now);
        previous.Activate(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var changes = JsonSerializer.Serialize(new { reason = request.Reason, fromVersion, toVersion = previous.PromptVersion });
        await auditLogWriter.WriteAsync(
            new AuditLogEntry(
                nameof(AiPrompt), previous.Id.Value.ToString(), "Rollback", request.ActorUserId, request.TenantId,
                now, changes, nameof(RollbackPromptVersionCommand), correlationIdProvider.CorrelationId, null),
            cancellationToken).ConfigureAwait(false);

        return Result.Success(AiPromptResponseMapper.ToResponse(previous));
    }
}
