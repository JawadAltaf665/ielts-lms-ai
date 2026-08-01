namespace IELTSLms.Domain.AI;

// Strongly-typed identifiers for the Conversation/Memory bounded context (Phase 7 Sprint 3), grouped in
// one file per the Billing/Grammar/AI convention — one-file-per-id doesn't scale without pure ceremony.

/// <summary>Strongly-typed identifier for a <see cref="Conversation"/>.</summary>
public readonly record struct ConversationId(Guid Value)
{
    public static ConversationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ConversationMessage"/>.</summary>
public readonly record struct ConversationMessageId(Guid Value)
{
    public static ConversationMessageId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <see cref="ConversationSummary"/>.</summary>
public readonly record struct ConversationSummaryId(Guid Value)
{
    public static ConversationSummaryId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
