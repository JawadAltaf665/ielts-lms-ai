using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.CreateGrammarTopic;

public sealed class CreateGrammarTopicCommandHandler(
    IGrammarTopicRepository topics, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateGrammarTopicCommand, GrammarTopicResponse>
{
    public async Task<Result<GrammarTopicResponse>> Handle(CreateGrammarTopicCommand request, CancellationToken cancellationToken)
    {
        // Topics created through the tenant-facing API are always tenant-owned; global-catalog
        // topics (TenantId null) are seeded content, not authored here.
        if (await topics.TitleExistsAsync(request.TenantId, request.Title, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<GrammarTopicResponse>(GrammarErrors.TopicTitleTaken);
        }

        var now = clock.UtcNow;
        var createResult = GrammarTopic.Create(
            GrammarTopicId.From(guids.NewGuid()), request.TenantId, request.Title, request.Description, request.Track,
            request.Difficulty, request.CategoryId, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<GrammarTopicResponse>(createResult.Error);
        }

        var topic = createResult.Value;
        topics.Add(topic);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(topic));
    }

    internal static GrammarTopicResponse ToResponse(GrammarTopic topic) => new(
        topic.Id.Value, topic.TenantId, topic.Title, topic.Description, topic.Track.ToString(), topic.Difficulty.ToString(),
        topic.CategoryId, topic.IsGlobal);
}
