using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Learning.Recommendations.Commands.DismissRecommendation;

/// <summary>Learner rejects the current active recommendation set (business rule: recorded as
/// <c>Dismissed</c>, feeding the next generation's "don't immediately re-recommend this" context) — an
/// action sub-resource beyond the sprint's literal endpoint table, added because the business rules and
/// required Tier-1 test explicitly call for a dismiss lifecycle.</summary>
public sealed record DismissRecommendationCommand(Guid TenantId, Guid UserId, Guid RecommendationId) : ICommand;
