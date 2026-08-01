namespace IELTSLms.Application.AI.Conversations;

/// <summary>Structured conversation-summarization output (mirrors <c>GrammarCoachOutput</c>'s shape).</summary>
public sealed record ConversationSummaryOutput(string Summary);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> so the provider's
/// structured output is shape-constrained (AI Architecture §8).</summary>
internal static class ConversationSummarySchema
{
    public const string Name = "record_conversation_summary";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "summary": { "type": "string", "description": "A compact running summary of the compacted turns." }
          },
          "required": ["summary"]
        }
        """;
}
