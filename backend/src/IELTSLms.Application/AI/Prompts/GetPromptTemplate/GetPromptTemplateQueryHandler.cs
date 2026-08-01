using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.GetPromptTemplate;

public sealed class GetPromptTemplateQueryHandler(IAiPromptRepository prompts)
    : IQueryHandler<GetPromptTemplateQuery, AiPromptResponse>
{
    public async Task<Result<AiPromptResponse>> Handle(GetPromptTemplateQuery request, CancellationToken cancellationToken)
    {
        var prompt = await prompts.GetByIdAsync(AiPromptId.From(request.PromptId), cancellationToken).ConfigureAwait(false);
        if (prompt is null || prompt.TenantId != request.TenantId)
        {
            return Result.Failure<AiPromptResponse>(AiErrors.PromptNotFound);
        }

        return Result.Success(AiPromptResponseMapper.ToResponse(prompt));
    }
}
