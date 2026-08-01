using IELTSLms.Domain.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Certificates;

internal sealed class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("achievements");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AchievementId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.AchievementType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.AwardedAtUtc).IsRequired();

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("achievements");

        // ERD ENT-054: unique per (UserId, AchievementType) where single-award — every type here is single-award this sprint.
        builder.HasIndex(a => new { a.TenantId, a.UserId, a.AchievementType })
            .IsUnique()
            .HasDatabaseName("ix_achievements_tenant_user_type_unique");

        builder.Ignore(a => a.DomainEvents);
    }
}
