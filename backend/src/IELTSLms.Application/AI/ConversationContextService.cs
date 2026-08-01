using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.Conversations;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI;

/// <summary>
/// ponytail: the token budget and recency floor are fixed constants, not per-tenant/per-feature
/// configuration — the sprint scope is context assembly + summarization triggering, not a tuning surface;
/// promote to configuration if a future capability needs a different budget.
/// </summary>
public sealed class ConversationContextService(
    IConversationRepository conversations,
    IConversationMessageRepository messages,
    IConversationSummaryRepository summaries,
    IAiPromptRepository prompts,
    IAiProviderRepository providers,
    IAiResponseRepository responses,
    IAiGateway gateway,
    IPromptCompositionService promptComposition,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids)
    : IConversationContextService
{
    private const int MaxHistoryTokens = 2000;
    private const int MinRecentMessagesKept = 6;
    private const int SummaryMaxOutputTokens = 512;

    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables = new Dictionary<string, string>();

    public async Task<Result<AssembledConversationContext>> AssembleAsync(
        Guid tenantId,
        Guid userId,
        ConversationId conversationId,
        AiPrompt prompt,
        IReadOnlyDictionary<string, string> systemVariables,
        string userContent,
        CancellationToken cancellationToken = default)
    {
        var conversation = await conversations.GetByIdAsync(conversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null || conversation.TenantId != tenantId || conversation.UserId != userId)
        {
            return Result.Failure<AssembledConversationContext>(AiErrors.ConversationNotFound);
        }

        var summary = await summaries.GetLatestAsync(tenantId, conversationId, cancellationToken).ConfigureAwait(false);
        var tail = await messages
            .ListSinceSequenceAsync(tenantId, conversationId, summary?.CoversUpToSequence ?? 0, cancellationToken)
            .ConfigureAwait(false);

        if (ExceedsBudget(tail))
        {
            var summarized = await SummarizeAsync(tenantId, conversationId, summary, tail, cancellationToken).ConfigureAwait(false);
            if (summarized.IsFailure)
            {
                return Result.Failure<AssembledConversationContext>(summarized.Error);
            }

            summary = summarized.Value;
            tail = await messages
                .ListSinceSequenceAsync(tenantId, conversationId, summary.CoversUpToSequence, cancellationToken)
                .ConfigureAwait(false);
        }

        var composed = promptComposition.Compose(prompt, systemVariables, userContent);
        if (composed.IsFailure)
        {
            return Result.Failure<AssembledConversationContext>(composed.Error);
        }

        var history = new List<ConversationContextTurn>();
        if (summary is not null)
        {
            history.Add(new ConversationContextTurn(ConversationMessageRole.System, summary.SummaryText));
        }

        history.AddRange(tail.Select(m => new ConversationContextTurn(m.Role, m.Content)));

        return Result.Success(new AssembledConversationContext(composed.Value.SystemPrompt, composed.Value.UserContent, history));
    }

    /// <summary>Summarize once the tail exceeds budget, but only if there is more than the recency floor
    /// to compact — business rule: the most recent <see cref="MinRecentMessagesKept"/> raw messages are
    /// never dropped, even under aggressive compaction.</summary>
    private static bool ExceedsBudget(IReadOnlyList<ConversationMessage> tail) =>
        tail.Count > MinRecentMessagesKept && tail.Sum(m => m.TokenCount) > MaxHistoryTokens;

    private async Task<Result<ConversationSummary>> SummarizeAsync(
        Guid tenantId, ConversationId conversationId, ConversationSummary? priorSummary,
        IReadOnlyList<ConversationMessage> tail, CancellationToken cancellationToken)
    {
        var toCompact = tail.Take(tail.Count - MinRecentMessagesKept).ToList();

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var summaryPrompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.ConversationSummary, cancellationToken).ConfigureAwait(false);
        if (provider is null || summaryPrompt is null)
        {
            return Result.Failure<ConversationSummary>(AiErrors.ConversationSummaryPromptNotConfigured);
        }

        var input = BuildSummarizationInput(priorSummary, toCompact);
        var composed = promptComposition.Compose(summaryPrompt, NoSystemVariables, input);
        if (composed.IsFailure)
        {
            return Result.Failure<ConversationSummary>(composed.Error);
        }

        var now = clock.UtcNow;
        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.ConversationSummary, summaryPrompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                composed.Value.UserContent, ConversationSummarySchema.Name, ConversationSummarySchema.Json, SummaryMaxOutputTokens),
            cancellationToken).ConfigureAwait(false);
        if (invocation.IsFailure)
        {
            return Result.Failure<ConversationSummary>(invocation.Error);
        }

        var result = invocation.Value;
        var summaryText = ParseSummaryText(result.OutputJson);
        var status = summaryText is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid;

        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, summaryPrompt.Id, summaryPrompt.PromptVersion,
            nameof(Conversation), conversationId.Value, ComputeInputHash(input), result.Model, result.PromptTokens,
            result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency, status,
            status == AiResponseStatus.Succeeded ? result.OutputJson : null, result.ProviderResponseId, now);
        if (responseCreate.IsFailure)
        {
            return Result.Failure<ConversationSummary>(responseCreate.Error);
        }

        responses.Add(responseCreate.Value);

        if (summaryText is null)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure<ConversationSummary>(AiErrors.InputRejected);
        }

        var summaryCreate = ConversationSummary.Create(
            ConversationSummaryId.From(guids.NewGuid()), tenantId, conversationId, summaryText, toCompact[^1].Sequence, now);
        if (summaryCreate.IsFailure)
        {
            return Result.Failure<ConversationSummary>(summaryCreate.Error);
        }

        summaries.Add(summaryCreate.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(summaryCreate.Value);
    }

    private static string BuildSummarizationInput(ConversationSummary? priorSummary, IReadOnlyList<ConversationMessage> toCompact)
    {
        var builder = new StringBuilder();
        if (priorSummary is not null)
        {
            builder.AppendLine("Prior summary:").AppendLine(priorSummary.SummaryText).AppendLine();
        }

        builder.AppendLine("Turns to compact:");
        foreach (var message in toCompact)
        {
            builder.Append(message.Role).Append(": ").AppendLine(message.Content);
        }

        return builder.ToString();
    }

    private static string? ParseSummaryText(string outputJson)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<ConversationSummaryOutput>(outputJson, JsonSerializerOptions.Web);
            return string.IsNullOrWhiteSpace(parsed?.Summary) ? null : parsed.Summary;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ComputeInputHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
