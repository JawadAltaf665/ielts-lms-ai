using System.Text;

namespace IELTSLms.Application.AI;

/// <summary>
/// Flattens an <see cref="AssembledConversationContext"/> into the single user-turn string
/// <see cref="Abstractions.AI.IAiGateway"/> actually accepts (Phase 7 Sprint 4) — the gateway port is a
/// single system+user string pair (AI Architecture §2), it does not take a multi-turn message array, so
/// every Sprint 3 <c>ConversationContextTurn</c> the context service assembled has to be rendered into
/// that one string before the new turn. Mirrors <c>ConversationContextService</c>'s own
/// <c>BuildSummarizationInput</c> shape rather than inventing a second transcript format.
/// </summary>
internal static class ConversationTranscriptFormatter
{
    public static string Flatten(AssembledConversationContext context)
    {
        if (context.History.Count == 0)
        {
            return context.UserContent;
        }

        var builder = new StringBuilder();
        foreach (var turn in context.History)
        {
            builder.Append(turn.Role).Append(": ").AppendLine(turn.Content);
        }

        builder.Append("User: ").Append(context.UserContent);
        return builder.ToString();
    }
}
