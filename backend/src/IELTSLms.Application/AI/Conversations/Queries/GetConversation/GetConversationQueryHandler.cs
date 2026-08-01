using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Conversations.Queries.GetConversation;

public sealed class GetConversationQueryHandler(IConversationRepository conversations, IConversationMessageRepository messages)
    : IQueryHandler<GetConversationQuery, ConversationDetailResponse>
{
    public async Task<Result<ConversationDetailResponse>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversations
            .GetByIdAsync(ConversationId.From(request.ConversationId), cancellationToken)
            .ConfigureAwait(false);
        if (conversation is null || conversation.TenantId != request.TenantId || conversation.UserId != request.UserId)
        {
            return Result.Failure<ConversationDetailResponse>(AiErrors.ConversationNotFound);
        }

        int? beforeSequence = ConversationMessageCursor.TryDecode(request.Cursor, out var sequence) ? sequence : null;

        var page = await messages
            .ListPageAsync(request.TenantId, conversation.Id, beforeSequence, request.Limit, cancellationToken)
            .ConfigureAwait(false);

        var hasMore = page.Count > request.Limit;
        var items = page.Take(request.Limit).ToList();
        var nextCursor = hasMore && items.Count > 0 ? ConversationMessageCursor.Encode(items[^1].Sequence) : null;

        var detail = new ConversationDetailResponse(
            ConversationResponseMapper.ToResponse(conversation),
            CursorPage<ConversationMessageResponse>.Create(items.Select(ConversationMessageResponseMapper.ToResponse).ToArray(), nextCursor));

        return Result.Success(detail);
    }
}
