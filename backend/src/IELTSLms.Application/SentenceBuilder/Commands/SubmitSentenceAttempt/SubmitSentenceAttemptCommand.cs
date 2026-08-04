using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;

/// <summary>
/// Submits and synchronously grades a constructed sentence (deterministic pattern match, no AI —
/// hence a direct <c>200</c> response, never <c>202</c>). Raises <c>SentenceAttemptSubmitted</c> on
/// success, whose handler kicks off the AI scaffolding enrichment asynchronously (business rules: "AI
/// is additive, not gating"). Mirrors <c>SubmitGrammarAttemptCommand</c>'s shape.
/// </summary>
public sealed record SubmitSentenceAttemptCommand(Guid TenantId, Guid UserId, Guid ExerciseId, string SubmittedText)
    : ICommand<SentenceAttemptResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["sentence-builder.attempt"];
}

public sealed record SentenceAttemptResponse(
    Guid Id, Guid ExerciseId, string SubmittedText, int? Score, Guid? AiResponseId, string? Feedback, DateTimeOffset SubmittedAtUtc);
