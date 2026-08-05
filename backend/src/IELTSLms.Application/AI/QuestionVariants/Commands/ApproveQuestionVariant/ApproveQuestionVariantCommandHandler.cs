using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.AddListeningQuestion;
using IELTSLms.Application.Reading.AddReadingQuestion;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.ApproveQuestionVariant;

public sealed class ApproveQuestionVariantCommandHandler(
    IQuestionVariantDraftRepository drafts, IReadingQuestionRepository readingQuestions,
    IListeningQuestionRepository listeningQuestions, ISender sender, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ApproveQuestionVariantCommand, QuestionVariantResponse>
{
    public async Task<Result<QuestionVariantResponse>> Handle(ApproveQuestionVariantCommand request, CancellationToken cancellationToken)
    {
        var draft = await drafts.GetByIdAsync(QuestionVariantDraftId.From(request.DraftId), cancellationToken).ConfigureAwait(false);
        if (draft is null)
        {
            return Result.Failure<QuestionVariantResponse>(QuestionVariantErrors.DraftNotFound);
        }

        // Checked before promoting, not just before the final Approve() call: promoting re-invokes
        // AddReadingQuestionCommand/AddListeningQuestionCommand, so re-approving an already-terminal
        // draft must not create a second, duplicate question before the status guard has a chance to fail.
        if (draft.Status != QuestionVariantStatus.Pending)
        {
            return Result.Failure<QuestionVariantResponse>(QuestionVariantErrors.DraftNotPending);
        }

        var content = JsonSerializer.Deserialize<QuestionVariantContent>(draft.DraftContent, JsonSerializerOptions.Web)!;
        var type = Enum.Parse<ReadingQuestionType>(content.Type);

        var promoteResult = draft.SourceType == QuestionVariantSourceType.Reading
            ? await PromoteToReadingAsync(request.TenantId, draft, content, type, cancellationToken).ConfigureAwait(false)
            : await PromoteToListeningAsync(request.TenantId, draft, content, type, cancellationToken).ConfigureAwait(false);
        if (promoteResult.IsFailure)
        {
            return Result.Failure<QuestionVariantResponse>(promoteResult.Error);
        }

        var approveResult = draft.Approve(clock.UtcNow);
        if (approveResult.IsFailure)
        {
            return Result.Failure<QuestionVariantResponse>(approveResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(QuestionVariantResponse.From(draft, content));
    }

    private async Task<Result> PromoteToReadingAsync(
        Guid tenantId, QuestionVariantDraft draft, QuestionVariantContent content, ReadingQuestionType type,
        CancellationToken cancellationToken)
    {
        var sourceQuestion = await readingQuestions
            .GetByIdAsync(ReadingQuestionId.From(draft.SourceQuestionId), cancellationToken).ConfigureAwait(false);
        if (sourceQuestion is null)
        {
            return Result.Failure(QuestionVariantErrors.SourceQuestionNotFound);
        }

        var siblingQuestions = await readingQuestions
            .ListByPassageAsync(sourceQuestion.ReadingPassageId, cancellationToken).ConfigureAwait(false);
        var nextSequence = siblingQuestions.Count == 0 ? 0 : siblingQuestions.Max(q => q.Sequence) + 1;

        var command = new AddReadingQuestionCommand(
            tenantId, sourceQuestion.ReadingPassageId.Value, type, content.Prompt, content.AnswerKey, nextSequence);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    private async Task<Result> PromoteToListeningAsync(
        Guid tenantId, QuestionVariantDraft draft, QuestionVariantContent content, ReadingQuestionType type,
        CancellationToken cancellationToken)
    {
        var sourceQuestion = await listeningQuestions
            .GetByIdAsync(ListeningQuestionId.From(draft.SourceQuestionId), cancellationToken).ConfigureAwait(false);
        if (sourceQuestion is null)
        {
            return Result.Failure(QuestionVariantErrors.SourceQuestionNotFound);
        }

        var siblingQuestions = await listeningQuestions
            .ListByAudioAsync(sourceQuestion.ListeningAudioId, cancellationToken).ConfigureAwait(false);
        var nextSequence = siblingQuestions.Count == 0 ? 0 : siblingQuestions.Max(q => q.Sequence) + 1;

        var command = new AddListeningQuestionCommand(
            tenantId, sourceQuestion.ListeningAudioId.Value, type, content.Prompt, content.AnswerKey, nextSequence);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}
