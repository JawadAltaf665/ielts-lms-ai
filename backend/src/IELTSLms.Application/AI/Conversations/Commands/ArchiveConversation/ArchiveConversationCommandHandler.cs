using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Conversations.Commands.ArchiveConversation;

public sealed class ArchiveConversationCommandHandler(IConversationRepository conversations, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ArchiveConversationCommand>
{
    public async Task<Result> Handle(ArchiveConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversations
            .GetByIdAsync(ConversationId.From(request.ConversationId), cancellationToken)
            .ConfigureAwait(false);
        if (conversation is null || conversation.TenantId != request.TenantId || conversation.UserId != request.UserId)
        {
            return Result.Failure(AiErrors.ConversationNotFound);
        }

        conversation.Archive(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
