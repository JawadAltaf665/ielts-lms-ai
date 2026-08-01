using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.DeleteGrammarTopic;

public sealed class DeleteGrammarTopicCommandHandler(IGrammarTopicRepository topics, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DeleteGrammarTopicCommand>
{
    public async Task<Result> Handle(DeleteGrammarTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await topics.GetVisibleByIdAsync(request.TenantId, GrammarTopicId.From(request.TopicId), cancellationToken)
            .ConfigureAwait(false);
        if (topic is null)
        {
            return Result.Failure(GrammarErrors.TopicNotFound);
        }

        if (topic.IsGlobal)
        {
            return Result.Failure(GrammarErrors.TopicReadOnly);
        }

        topic.SoftDelete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
