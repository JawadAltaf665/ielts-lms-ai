using IELTSLms.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Audit;

internal sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, v => new ActivityLogId(v));

        builder.Property(x => x.ActivityType).HasColumnName("activity_type").HasMaxLength(64).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128);

        builder.HasIndex(x => x.ActivityType).HasDatabaseName("ix_activity_logs_type");
        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_activity_logs_user_id");
        builder.HasIndex(x => x.Timestamp).HasDatabaseName("ix_activity_logs_timestamp");
    }
}
