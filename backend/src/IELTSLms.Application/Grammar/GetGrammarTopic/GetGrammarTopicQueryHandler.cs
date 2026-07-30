using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.GetGrammarTopic;

public sealed class GetGrammarTopicQueryHandler(IGrammarTopicRepository topics, IGrammarExerciseRepository exercises)
    : IQueryHandler<GetGrammarTopicQuery, GrammarTopicDetailResponse>
{
    public async Task<Result<GrammarTopicDetailResponse>> Handle(GetGrammarTopicQuery request, CancellationToken cancellationToken)
    {
        var topic = await topics.GetVisibleByIdAsync(request.TenantId, GrammarTopicId.From(request.TopicId), cancellationToken)
            .ConfigureAwait(false);
        if (topic is null)
        {
            return Result.Failure<GrammarTopicDetailResponse>(GrammarErrors.TopicNotFound);
        }

        var topicExercises = await exercises.ListByTopicAsync(request.TenantId, topic.Id, cancellationToken).ConfigureAwait(false);

        return Result.Success(new GrammarTopicDetailResponse(
            topic.Id.Value, topic.TenantId, topic.Title, topic.Description, topic.Track.ToString(), topic.Difficulty.ToString(),
            topic.CategoryId, topic.IsGlobal,
            topicExercises.Select(e => new GrammarExerciseSummaryResponse(e.Id.Value, e.Title, e.Mode.ToString(), e.Order)).ToArray()));
    }
}
