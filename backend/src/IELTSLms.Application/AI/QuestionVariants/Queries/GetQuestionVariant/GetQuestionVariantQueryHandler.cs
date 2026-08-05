using System.Text.Json;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.QuestionVariants.Queries.GetQuestionVariant;

public sealed class GetQuestionVariantQueryHandler(IQuestionVariantDraftRepository drafts)
    : IQueryHandler<GetQuestionVariantQuery, QuestionVariantResponse>
{
    public async Task<Result<QuestionVariantResponse>> Handle(GetQuestionVariantQuery request, CancellationToken cancellationToken)
    {
        var draft = await drafts.GetByIdAsync(QuestionVariantDraftId.From(request.DraftId), cancellationToken).ConfigureAwait(false);
        if (draft is null)
        {
            return Result.Failure<QuestionVariantResponse>(QuestionVariantErrors.DraftNotFound);
        }

        var content = JsonSerializer.Deserialize<QuestionVariantContent>(draft.DraftContent, JsonSerializerOptions.Web)!;
        return Result.Success(QuestionVariantResponse.From(draft, content));
    }
}
