using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Learning;

internal sealed class ProgressTrackingConfiguration : IEntityTypeConfiguration<ProgressTracking>
{
    public void Configure(EntityTypeBuilder<ProgressTracking> builder)
    {
        builder.ToTable("progress_tracking");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ProgressTrackingId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.Skill).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(p => p.ActivityCount).IsRequired();
        builder.Property(p => p.LastActivityAtUtc);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("progress_tracking");

        builder.HasIndex(p => new { p.TenantId, p.UserId, p.Skill })
            .IsUnique()
            .HasDatabaseName("ix_progress_tracking_tenant_user_skill_unique");

        builder.Ignore(p => p.DomainEvents);
    }
}
