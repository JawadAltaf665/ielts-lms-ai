namespace IELTSLms.Application.AI;

/// <summary>
/// Estimates <see cref="Domain.AI.ConversationMessage.TokenCount"/> at write time (Phase 7 Sprint 4 —
/// the first callers that actually write messages). ponytail: a chars/4 heuristic, not a real
/// tokenizer — no tokenizer package exists in this project's dependency set
/// (<c>Directory.Packages.props</c>) yet, and the field only feeds <c>ConversationContextService</c>'s
/// own budget check, which already tolerates approximate counts. Swap in the provider's actual token
/// count (already returned per-response by <c>IAiGateway</c>) if the estimate proves too coarse.
/// </summary>
internal static class ConversationMessageTokenEstimator
{
    public static int Estimate(string text) => Math.Max(1, text.Length / 4);
}
