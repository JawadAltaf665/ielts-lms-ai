using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class UsageRecordConfiguration : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(EntityTypeBuilder<UsageRecord> builder)
    {
        builder.ToTable("usage_records");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => UsageRecordId.From(value))
            .ValueGeneratedNever();

        builder.Property(u => u.QuotaDefinitionId)
            .HasConversion(id => id.Value, value => QuotaDefinitionId.From(value))
            .IsRequired();

        builder.Property(u => u.PeriodStart).IsRequired();
        builder.Property(u => u.PeriodEnd).IsRequired();
        builder.Property(u => u.Consumed).IsRequired();

        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.UpdatedAtUtc);
        builder.Property(u => u.IsDeleted).IsRequired();
        builder.Property(u => u.DeletedAtUtc);

        builder.MapXminConcurrencyToken(u => u.Version);
        builder.MapTenantScope("usage_records");

        builder.HasIndex(u => new { u.TenantId, u.QuotaDefinitionId, u.PeriodStart })
            .IsUnique()
            .HasDatabaseName("ix_usage_records_tenant_quota_period");

        builder.Ignore(u => u.DomainEvents);
    }
}
