using IELTSLms.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Audit;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, v => new AuditLogId(v));

        builder.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(32).HasConversion<string>().IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.Property(x => x.Changes).HasColumnName("changes").HasColumnType("jsonb");
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(128);
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128);
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);

        builder.HasIndex(x => x.EntityName).HasDatabaseName("ix_audit_logs_entity_name");
        builder.HasIndex(x => new { x.EntityName, x.EntityId }).HasDatabaseName("ix_audit_logs_entity_lookup");
        builder.HasIndex(x => x.Timestamp).HasDatabaseName("ix_audit_logs_timestamp");
    }
}
