using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Learning.WeaknessDetection.Commands.RefreshWeaknessProfile;

/// <summary>Learner-triggered force recompute (distinct from the daily scheduled recompute) — returns
/// <c>202</c> + an operation resource, same shape as <c>RequestReadingCoachingCommand</c>.</summary>
public sealed record RefreshWeaknessProfileCommand(Guid TenantId, Guid UserId) : ICommand<AiOperationAcceptedResponse>;
