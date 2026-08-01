using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Certificates;

/// <summary>
/// A criteria-driven, idempotently-awarded gamification badge (ERD ENT-054, DDD §13.16 BC-16).
/// Idempotent awarding (ERD ENT-054: unique per (<see cref="UserId"/>, <see cref="AchievementType"/>)
/// where single-award) is the caller's responsibility — <c>CertificateIssuanceService</c> checks for
/// an existing row before calling <see cref="Award"/>, the same "check then create" shape
/// <c>ProgressTrackingService</c> already uses.
/// </summary>
public sealed class Achievement : TenantAuditableEntity<AchievementId>
{
    private Achievement(AchievementId id, Guid tenantId, Guid userId, string achievementType, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        AchievementType = achievementType;
        AwardedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private Achievement()
    {
        AchievementType = null!;
    }

    public Guid UserId { get; private init; }

    public string AchievementType { get; private init; }

    public DateTimeOffset AwardedAtUtc { get; private init; }

    public static Achievement Award(AchievementId id, Guid tenantId, Guid userId, string achievementType, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, achievementType, nowUtc);
}
