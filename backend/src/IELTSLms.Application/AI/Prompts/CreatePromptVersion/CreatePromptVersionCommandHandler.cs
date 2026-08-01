using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.CreatePromptVersion;

public sealed class CreatePromptVersionCommandHandler(IAiPromptRepository prompts, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreatePromptVersionCommand, AiPromptResponse>
{
    public async Task<Result<AiPromptResponse>> Handle(CreatePromptVersionCommand request, CancellationToken cancellationToken)
    {
        var scope = string.IsNullOrWhiteSpace(request.Scope) ? "default" : request.Scope.Trim();

        var existingVersions = await prompts
            .GetVersionsAsync(request.TenantId, request.Feature, scope, cancellationToken)
            .ConfigureAwait(false);
        var nextVersion = existingVersions.Count == 0 ? 1 : existingVersions[0].PromptVersion + 1;

        var providerId = request.AiProviderId is { } id ? AiProviderId.From(id) : (AiProviderId?)null;
        var created = AiPrompt.Create(
            AiPromptId.From(guids.NewGuid()), request.TenantId, providerId, request.Feature, nextVersion, scope,
            request.SystemTemplate, clock.UtcNow);
        if (created.IsFailure)
        {
            return Result.Failure<AiPromptResponse>(created.Error);
        }

        prompts.Add(created.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(AiPromptResponseMapper.ToResponse(created.Value));
    }
}
