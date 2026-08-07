using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// The compaction artifact for a long-running <see cref="Conversation"/> (Phase 7 Sprint 3, AI
/// Architecture §6): replaces raw <see cref="ConversationMessage"/> rows up to
/// <see cref="CoversUpToSequence"/> in context assembly. Write-once/immutable, same shape as
/// <see cref="ConversationMessage"/> — a re-summarization never edits a prior summary, it persists a new
/// row with a higher <see cref="CoversUpToSequence"/>; context assembly always reads the latest one.
/// </summary>
public sealed class ConversationSummary : TenantAuditableEntity<ConversationSummaryId>
{
    private ConversationSummary(
        ConversationSummaryId id, Guid tenantId, ConversationId conversationId, string summaryText,
        int coversUpToSequence, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        SummaryText = summaryText;
        CoversUpToSequence = coversUpToSequence;
        CreatedAtUtc = nowUtc;
    }

    private ConversationSummary()
    {
        SummaryText = null!;
    }

    public ConversationId ConversationId { get; private init; }

    public string SummaryText { get; private init; }

    /// <summary>The last raw <see cref="ConversationMessage.Sequence"/> this summary replaces in
    /// context assembly; messages after this sequence are still read raw.</summary>
    public int CoversUpToSequence { get; private init; }

    public static Result<ConversationSummary> Create(
        ConversationSummaryId id, Guid tenantId, ConversationId conversationId, string summaryText,
        int coversUpToSequence, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(summaryText))
        {
            return Result.Failure<ConversationSummary>(ConversationDomainErrors.SummaryTextRequired);
        }

        if (coversUpToSequence <= 0)
        {
            return Result.Failure<ConversationSummary>(ConversationDomainErrors.SummaryCoversUpToSequenceInvalid);
        }

        return Result.Success(new ConversationSummary(id, tenantId, conversationId, summaryText, coversUpToSequence, nowUtc));
    }
}
