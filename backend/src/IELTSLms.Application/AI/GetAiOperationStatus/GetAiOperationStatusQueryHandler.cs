using System.Text.Json;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.GrammarCoach;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.GetAiOperationStatus;

public sealed class GetAiOperationStatusQueryHandler(IAiOperationRepository operations, IAiResponseRepository responses)
    : IQueryHandler<GetAiOperationStatusQuery, AiOperationStatusResponse>
{
    public async Task<Result<AiOperationStatusResponse>> Handle(GetAiOperationStatusQuery request, CancellationToken cancellationToken)
    {
        var operation = await operations
            .GetByIdAsync(AiOperationId.From(request.OperationId), cancellationToken)
            .ConfigureAwait(false);
        if (operation is null || operation.UserId != request.UserId)
        {
            return Result.Failure<AiOperationStatusResponse>(AiErrors.OperationNotFound);
        }

        GrammarCoachOutput? result = null;
        if (operation.Status == AiOperationStatus.Succeeded && operation.AiResponseId is { } aiResponseId)
        {
            var aiResponse = await responses.GetByIdAsync(aiResponseId, cancellationToken).ConfigureAwait(false);
            if (aiResponse?.OutputJson is not null)
            {
                result = JsonSerializer.Deserialize<GrammarCoachOutput>(aiResponse.OutputJson, JsonSerializerOptions.Web);
            }
        }

        return Result.Success(new AiOperationStatusResponse(
            operation.Id.Value, operation.Kind, operation.Status.ToString().ToLowerInvariant(), result,
            operation.ErrorCode, operation.ErrorMessage));
    }
}
