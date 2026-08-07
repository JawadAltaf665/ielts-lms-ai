namespace IELTSLms.Application.AI.Tutor;

/// <summary>Structured AI Tutor answer output (mirrors <c>GrammarCoachOutput</c>'s shape). <see cref="InScope"/>
/// is the model's own self-report — an audit signal, not the enforcement mechanism; <see cref="TutorScopeGuard"/>
/// is the actual code-level gate and runs before the gateway is ever called.</summary>
public sealed record TutorAnswerOutput(string Answer, bool InScope);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> (AI Architecture §8).</summary>
internal static class TutorAnswerSchema
{
    public const string Name = "record_tutor_answer";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "answer": { "type": "string", "description": "The tutor's answer to the learner's question, or a polite redirect if the question is outside IELTS preparation." },
            "inScope": { "type": "boolean", "description": "Whether the learner's question was within IELTS-preparation scope." }
          },
          "required": ["answer", "inScope"]
        }
        """;
}
