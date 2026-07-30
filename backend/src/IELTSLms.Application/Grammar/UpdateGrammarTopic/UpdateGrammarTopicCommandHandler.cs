using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.UpdateGrammarTopic;

/// <remarks>
/// Concurrency here relies on the natural EF/xmin optimistic-concurrency check (load, mutate, save
/// within one request) — the same as every other "update" command in this codebase (e.g. Billing's
/// ChangePlanCommand). A client-supplied <c>If-Match</c>/<c>Version</c> check would need generic
/// ETag middleware that doesn't exist anywhere yet; ponytail: add if a real staleness bug shows up.
/// </remarks>
public sealed class UpdateGrammarTopicCommandHandler(IGrammarTopicRepository topics, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UpdateGrammarTopicCommand, GrammarTopicResponse>
{
    public async Task<Result<GrammarTopicResponse>> Handle(UpdateGrammarTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await topics.GetVisibleByIdAsync(request.TenantId, GrammarTopicId.From(request.TopicId), cancellationToken)
            .ConfigureAwait(false);
        if (topic is null)
        {
            return Result.Failure<GrammarTopicResponse>(GrammarErrors.TopicNotFound);
        }

        if (topic.IsGlobal)
        {
            return Result.Failure<GrammarTopicResponse>(GrammarErrors.TopicReadOnly);
        }

        if (!string.Equals(topic.Title, request.Title, StringComparison.Ordinal)
            && await topics.TitleExistsAsync(topic.TenantId, request.Title, topic.Id, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<GrammarTopicResponse>(GrammarErrors.TopicTitleTaken);
        }

        var updateResult = topic.Update(request.Title, request.Description, request.Track, request.Difficulty, request.CategoryId, clock.UtcNow);
        if (updateResult.IsFailure)
        {
            return Result.Failure<GrammarTopicResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(CreateGrammarTopicCommandHandler.ToResponse(topic));
    }
}
