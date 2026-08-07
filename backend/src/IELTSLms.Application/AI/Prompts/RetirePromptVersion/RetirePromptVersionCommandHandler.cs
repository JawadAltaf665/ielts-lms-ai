using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.RetirePromptVersion;

public sealed class RetirePromptVersionCommandHandler(IAiPromptRepository prompts, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RetirePromptVersionCommand>
{
    public async Task<Result> Handle(RetirePromptVersionCommand request, CancellationToken cancellationToken)
    {
        var prompt = await prompts.GetByIdAsync(AiPromptId.From(request.PromptId), cancellationToken).ConfigureAwait(false);
        if (prompt is null || prompt.TenantId != request.TenantId)
        {
            return Result.Failure(AiErrors.PromptNotFound);
        }

        if (prompt.IsActive)
        {
            return Result.Failure(AiErrors.PromptActiveCannotRetire);
        }

        prompt.SoftDelete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
