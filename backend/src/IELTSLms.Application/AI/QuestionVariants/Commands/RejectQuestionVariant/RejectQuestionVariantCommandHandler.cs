using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.RejectQuestionVariant;

public sealed class RejectQuestionVariantCommandHandler(IQuestionVariantDraftRepository drafts, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RejectQuestionVariantCommand, QuestionVariantResponse>
{
    public async Task<Result<QuestionVariantResponse>> Handle(RejectQuestionVariantCommand request, CancellationToken cancellationToken)
    {
        var draft = await drafts.GetByIdAsync(QuestionVariantDraftId.From(request.DraftId), cancellationToken).ConfigureAwait(false);
        if (draft is null)
        {
            return Result.Failure<QuestionVariantResponse>(QuestionVariantErrors.DraftNotFound);
        }

        var rejectResult = draft.Reject(clock.UtcNow);
        if (rejectResult.IsFailure)
        {
            return Result.Failure<QuestionVariantResponse>(rejectResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var content = JsonSerializer.Deserialize<QuestionVariantContent>(draft.DraftContent, JsonSerializerOptions.Web)!;
        return Result.Success(QuestionVariantResponse.From(draft, content));
    }
}
