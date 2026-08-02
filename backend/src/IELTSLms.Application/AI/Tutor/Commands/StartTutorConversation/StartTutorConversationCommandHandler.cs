using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Conversations;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Tutor.Commands.StartTutorConversation;

public sealed class StartTutorConversationCommandHandler(
    IConversationRepository conversations, IUnitOfWork unitOfWork, IEntitlementService entitlements,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<StartTutorConversationCommand, ConversationResponse>
{
    public async Task<Result<ConversationResponse>> Handle(StartTutorConversationCommand request, CancellationToken cancellationToken)
    {
        var entitled = await entitlements.HasFeatureAsync(request.TenantId, AiFeatures.AiTutor, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<ConversationResponse>(AiErrors.FeatureNotEntitled);
        }

        var now = clock.UtcNow;
        var conversation = Conversation.Create(
            ConversationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTutor, now).Value;

        conversations.Add(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ConversationResponseMapper.ToResponse(conversation));
    }
}
