namespace IELTSLms.Application.AI.QuestionVariants;

/// <summary>The AI-generated part of a variant: a rephrased prompt and its answer key at the requested
/// difficulty — the question <em>type</em> is carried over unchanged from the source question (not
/// regenerated), so the provider only owns wording/difficulty, not the item's fundamental shape.</summary>
public sealed record QuestionVariantOutput(string Prompt, IReadOnlyList<string> AnswerKey);

/// <summary>The JSON Schema handed to <see cref="Abstractions.AI.IAiGateway"/> for question-variant
/// generation — same mechanism as <c>GrammarCoachSchema</c>.</summary>
internal static class QuestionVariantSchema
{
    public const string Name = "record_question_variant";

    public const string Json = """
        {
          "type": "object",
          "properties": {
            "prompt": { "type": "string", "description": "The rephrased question prompt at the requested difficulty." },
            "answerKey": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Acceptable answers for the rephrased prompt, same shape as the source question's answer key."
            }
          },
          "required": ["prompt", "answerKey"]
        }
        """;
}

/// <summary>The full stored shape of <see cref="Domain.AI.QuestionVariantDraft.DraftContent"/> — the
/// AI-generated <see cref="QuestionVariantOutput"/> plus the type carried over from the source question,
/// i.e. exactly the shape a Reading/Listening question is authored with
/// (<c>AddReadingQuestionCommand</c>/<c>AddListeningQuestionCommand</c>).</summary>
public sealed record QuestionVariantContent(string Type, string Prompt, IReadOnlyList<string> AnswerKey);
