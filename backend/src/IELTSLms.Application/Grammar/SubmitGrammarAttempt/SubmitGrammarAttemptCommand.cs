using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Grammar.SubmitGrammarAttempt;

/// <summary>
/// Submits and synchronously grades an attempt (rule-based answer-key matching, no AI — hence a
/// direct response, never <c>202</c>). Raises <c>GrammarCompleted</c> on success.
/// </summary>
public sealed record SubmitGrammarAttemptCommand(Guid TenantId, Guid UserId, Guid ExerciseId, IReadOnlyList<GrammarAnswerSubmission> Answers)
    : ICommand<GrammarAttemptResponse>;

public sealed record GrammarAnswerSubmission(Guid QuestionId, string? Answer);

public sealed record GrammarAttemptResponse(
    Guid Id, Guid ExerciseId, int RawScore, int MaxScore, IReadOnlyList<GrammarAttemptAnswerResponse> Answers, DateTimeOffset SubmittedAtUtc);

public sealed record GrammarAttemptAnswerResponse(Guid QuestionId, string? SubmittedAnswer, bool IsCorrect);
