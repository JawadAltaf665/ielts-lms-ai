using IELTSLms.Domain.Certificates;

namespace IELTSLms.Domain.Tests.Certificates;

public sealed class AchievementTests
{
    [Fact]
    public void Award_records_the_type_and_award_time()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var achievement = Achievement.Award(
            AchievementId.From(Guid.NewGuid()), tenantId, userId, AchievementType.CertificateEarned, now);

        Assert.Equal(tenantId, achievement.TenantId);
        Assert.Equal(userId, achievement.UserId);
        Assert.Equal(AchievementType.CertificateEarned, achievement.AchievementType);
        Assert.Equal(now, achievement.AwardedAtUtc);
    }
}
