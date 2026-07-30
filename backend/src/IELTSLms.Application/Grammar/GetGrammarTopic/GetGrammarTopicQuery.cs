using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Grammar.GetGrammarTopic;

/// <summary>Gets a single grammar topic plus its exercises (title/mode/order — not the questions).</summary>
public sealed record GetGrammarTopicQuery(Guid TenantId, Guid TopicId) : IQuery<GrammarTopicDetailResponse>;

public sealed record GrammarTopicDetailResponse(
    Guid Id, Guid? TenantId, string Title, string Description, string Track, string Difficulty, Guid? CategoryId,
    bool IsGlobal, IReadOnlyList<GrammarExerciseSummaryResponse> Exercises);

public sealed record GrammarExerciseSummaryResponse(Guid Id, string Title, string Mode, int Order);
