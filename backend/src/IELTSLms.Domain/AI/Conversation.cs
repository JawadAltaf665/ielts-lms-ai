using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A bounded dialog thread between a learner and an AI capability (ai-tutor, ai-teacher, ...) — the
/// short-term memory tier of Phase 7 Sprint 3 (AI Architecture §6, Agentic AI Architecture §5). A
/// learner may have multiple conversations per <see cref="Feature"/> (e.g. one per lesson topic), so
/// there is deliberately no <c>(UserId, Feature)</c> uniqueness constraint, only an index.
/// <see cref="Feature"/> is immutable after creation (mirrors <see cref="AiPrompt.Feature"/> tagging) —
/// a conversation cannot switch capabilities mid-thread since prompt/behavior scoping depends on it
/// staying stable per message.
/// </summary>
public sealed class Conversation : TenantAuditableEntity<ConversationId>
{
    private Conversation(ConversationId id, Guid tenantId, Guid userId, string feature, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Feature = feature;
        Status = ConversationStatus.Active;
        StartedAtUtc = nowUtc;
        LastMessageAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private Conversation()
    {
        Feature = null!;
    }

    public Guid UserId { get; private init; }

    /// <summary>Capability tag, e.g. <c>"ai-tutor"</c>, <c>"ai-teacher"</c>. Immutable after creation.</summary>
    public string Feature { get; private init; }

    public ConversationStatus Status { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private init; }

    public DateTimeOffset LastMessageAtUtc { get; private set; }

    public static Result<Conversation> Create(ConversationId id, Guid tenantId, Guid userId, string feature, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(feature))
        {
            return Result.Failure<Conversation>(ConversationDomainErrors.ConversationFeatureRequired);
        }

        return Result.Success(new Conversation(id, tenantId, userId, feature.Trim(), nowUtc));
    }

    /// <summary>Archives (soft-deletes) the conversation — learner-initiated cleanup, not a hard purge
    /// (retention policy still applies). Idempotent: archiving an already-archived conversation is a no-op.</summary>
    public void Archive(DateTimeOffset nowUtc)
    {
        if (Status == ConversationStatus.Archived)
        {
            return;
        }

        Status = ConversationStatus.Archived;
        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}

internal static class ConversationDomainErrors
{
    public static readonly Error ConversationFeatureRequired = Error.Validation(
        "VALIDATION_CONVERSATION_FEATURE_REQUIRED", "Conversation feature is required.");

    public static readonly Error MessageContentRequired = Error.Validation(
        "VALIDATION_CONVERSATION_MESSAGE_CONTENT_REQUIRED", "Message content is required.");

    public static readonly Error MessageSequenceInvalid = Error.Validation(
        "VALIDATION_CONVERSATION_MESSAGE_SEQUENCE_INVALID", "Message sequence must be positive.");

    public static readonly Error MessageTokenCountInvalid = Error.Validation(
        "VALIDATION_CONVERSATION_MESSAGE_TOKEN_COUNT_INVALID", "Message token count cannot be negative.");

    public static readonly Error SummaryTextRequired = Error.Validation(
        "VALIDATION_CONVERSATION_SUMMARY_TEXT_REQUIRED", "Summary text is required.");

    public static readonly Error SummaryCoversUpToSequenceInvalid = Error.Validation(
        "VALIDATION_CONVERSATION_SUMMARY_COVERS_UP_TO_SEQUENCE_INVALID", "Summary CoversUpToSequence must be positive.");
}
