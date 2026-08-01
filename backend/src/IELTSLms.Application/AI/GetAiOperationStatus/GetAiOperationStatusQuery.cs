using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.GrammarCoach;

namespace IELTSLms.Application.AI.GetAiOperationStatus;

/// <summary>
/// Generic poll endpoint for any async AI operation (API Design Standards §14.2). Scoped to the
/// caller's own operation — mirrors Grammar's <c>GetGrammarAttemptQuery</c> self-service pattern.
/// </summary>
public sealed record GetAiOperationStatusQuery(Guid TenantId, Guid UserId, Guid OperationId) : IQuery<AiOperationStatusResponse>;

public sealed record AiOperationStatusResponse(
    Guid Id, string Kind, string Status, GrammarCoachOutput? Result, string? ErrorCode, string? ErrorMessage);
