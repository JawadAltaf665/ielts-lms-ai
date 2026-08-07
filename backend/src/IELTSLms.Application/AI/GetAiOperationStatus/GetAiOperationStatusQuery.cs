using System.Text.Json;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.GetAiOperationStatus;

/// <summary>
/// Generic poll endpoint for any async AI operation (API Design Standards §14.2). Scoped to the
/// caller's own operation — mirrors Grammar's <c>GetGrammarAttemptQuery</c> self-service pattern.
/// </summary>
public sealed record GetAiOperationStatusQuery(Guid TenantId, Guid UserId, Guid OperationId) : IQuery<AiOperationStatusResponse>;

/// <summary>
/// <paramref name="Result"/> is the raw parsed <c>AiResponse.OutputJson</c>, not a capability-specific
/// type: this endpoint is shared by every AI capability (Grammar/Vocabulary/Reading/Listening Coach,
/// Sentence Builder AI, ...), each with its own output shape, so it cannot hardcode one of them
/// (Sprint 8 fix — previously hardcoded to <c>GrammarCoachOutput</c>, silently discarding every other
/// capability's result when polled through this endpoint). Callers that know which capability they
/// requested deserialize <see cref="JsonElement"/> into that capability's own output record.
/// </summary>
public sealed record AiOperationStatusResponse(
    Guid Id, string Kind, string Status, JsonElement? Result, string? ErrorCode, string? ErrorMessage);
