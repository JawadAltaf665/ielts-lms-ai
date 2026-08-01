namespace IELTSLms.Domain.AI;

/// <summary>Lifecycle of a <see cref="Conversation"/> (Phase 7 Sprint 3, AI Architecture §6).</summary>
public enum ConversationStatus
{
    Active = 1,
    Archived = 2,
}

/// <summary>Speaker of a <see cref="ConversationMessage"/> turn (Phase 7 Sprint 3, AI Architecture §6).</summary>
public enum ConversationMessageRole
{
    User = 1,
    Assistant = 2,
    System = 3,
}
