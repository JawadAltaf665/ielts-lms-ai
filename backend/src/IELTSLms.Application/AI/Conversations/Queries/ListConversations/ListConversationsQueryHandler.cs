using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Conversations.Queries.ListConversations;

public sealed class ListConversationsQueryHandler(IConversationRepository conversations)
    : IQueryHandler<ListConversationsQuery, CursorPage<ConversationResponse>>
{
    public async Task<Result<CursorPage<ConversationResponse>>> Handle(
        ListConversationsQuery request, CancellationToken cancellationToken)
    {
        ConversationStatus? status = request.Status is not null && Enum.TryParse<ConversationStatus>(request.Status, ignoreCase: true, out var parsed)
            ? parsed
            : null;

        var all = await conversations
            .ListByUserAsync(request.TenantId, request.UserId, request.Feature, status, cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<Conversation> ordered = all;
        if (ConversationCursor.TryDecode(request.Cursor, out var cursorAtUtc, out var cursorId))
        {
            ordered = ordered.Where(c =>
                c.LastMessageAtUtc < cursorAtUtc || (c.LastMessageAtUtc == cursorAtUtc && c.Id.Value.CompareTo(cursorId) < 0));
        }

        var page = ordered.Take(request.Limit + 1).ToList();
        var hasMore = page.Count > request.Limit;
        var items = page.Take(request.Limit).ToList();

        var nextCursor = hasMore && items.Count > 0
            ? ConversationCursor.Encode(items[^1].LastMessageAtUtc, items[^1].Id.Value)
            : null;

        return Result.Success(CursorPage<ConversationResponse>.Create(
            items.Select(ConversationResponseMapper.ToResponse).ToArray(), nextCursor));
    }
}
