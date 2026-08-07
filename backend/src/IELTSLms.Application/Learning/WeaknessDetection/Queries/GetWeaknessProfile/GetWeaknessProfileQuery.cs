using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Learning.WeaknessDetection.Queries.GetWeaknessProfile;

public sealed record GetWeaknessProfileQuery(Guid TenantId, Guid UserId) : IQuery<WeaknessProfileResponse>;

/// <param name="Status">"Ready" (a profile exists), "InsufficientData" (below the minimum-data
/// threshold — business rule), or "NotComputedYet" (enough data, but no scheduled/triggered
/// computation has run yet).</param>
public sealed record WeaknessProfileResponse(
    Guid? WeaknessProfileId, string Status, IReadOnlyList<SkillWeaknessScoreDto>? Scores, DateTimeOffset? ComputedAtUtc);
