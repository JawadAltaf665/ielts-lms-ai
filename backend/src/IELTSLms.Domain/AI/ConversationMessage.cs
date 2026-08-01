using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// One turn in a <see cref="Conversation"/> (Phase 7 Sprint 3, AI Architecture §6). Write-once/immutable
/// once written — mirrors <see cref="AiResponse"/>'s append-only shape (ERD ENT-047 <c>ActivityLogs</c>
/// partition-candidate posture): there is no mutation method, only the <see cref="Create"/> factory. A
/// correction is always a new message, never an edit. <see cref="TokenCount"/> is captured at write time
/// (not recomputed later) so context-budget accounting never needs to re-tokenize history.
/// <see cref="AiResponseId"/> links an assistant turn back to the gateway call that produced it, for
/// cost/audit traceability; <see langword="null"/> for user/system turns.
/// </summary>
public sealed class ConversationMessage : TenantAuditableEntity<ConversationMessageId>
{
    private ConversationMessage(
        ConversationMessageId id, Guid tenantId, ConversationId conversationId, ConversationMessageRole role,
        string content, int sequence, int tokenCount, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        Role = role;
        Content = content;
        Sequence = sequence;
        TokenCount = tokenCount;
        AiResponseId = aiResponseId;
        CreatedAtUtc = nowUtc;
    }

    private ConversationMessage()
    {
        Content = null!;
    }

    public ConversationId ConversationId { get; private init; }

    public ConversationMessageRole Role { get; private init; }

    public string Content { get; private init; }

    /// <summary>1-based, monotonically increasing per <see cref="ConversationId"/> — the natural
    /// ordering key context assembly and pagination both rely on.</summary>
    public int Sequence { get; private init; }

    public int TokenCount { get; private init; }

    public AiResponseId? AiResponseId { get; private init; }

    public static Result<ConversationMessage> Create(
        ConversationMessageId id, Guid tenantId, ConversationId conversationId, ConversationMessageRole role,
        string content, int sequence, int tokenCount, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<ConversationMessage>(ConversationDomainErrors.MessageContentRequired);
        }

        if (sequence <= 0)
        {
            return Result.Failure<ConversationMessage>(ConversationDomainErrors.MessageSequenceInvalid);
        }

        if (tokenCount < 0)
        {
            return Result.Failure<ConversationMessage>(ConversationDomainErrors.MessageTokenCountInvalid);
        }

        return Result.Success(new ConversationMessage(id, tenantId, conversationId, role, content, sequence, tokenCount, aiResponseId, nowUtc));
    }
}
