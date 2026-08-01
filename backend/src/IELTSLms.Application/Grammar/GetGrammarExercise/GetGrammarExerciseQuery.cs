using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Grammar.GetGrammarExercise;

/// <summary>Gets an exercise and its questions. The answer key is never included in this response
/// (no question-editing endpoint exists this sprint to need it back) — strips it for every caller,
/// which is the strict reading of "hidden from learners" without needing a manage-role check.</summary>
public sealed record GetGrammarExerciseQuery(Guid TenantId, Guid ExerciseId) : IQuery<GrammarExerciseDetailResponse>;

public sealed record GrammarExerciseDetailResponse(
    Guid Id, Guid TopicId, string Title, string Mode, int Order, IReadOnlyList<GrammarQuestionResponse> Questions);

public sealed record GrammarQuestionResponse(Guid Id, string Type, string Prompt, int Sequence);
