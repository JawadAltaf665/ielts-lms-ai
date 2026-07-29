using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class TenantQuotaConfiguration : IEntityTypeConfiguration<TenantQuota>
{
    public void Configure(EntityTypeBuilder<TenantQuota> builder)
    {
        builder.ToTable("tenant_quotas");

        builder.HasKey(tq => tq.Id);
        builder.Property(tq => tq.Id)
            .HasConversion(id => id.Value, value => TenantQuotaId.From(value))
            .ValueGeneratedNever();

        builder.Property(tq => tq.QuotaDefinitionId)
            .HasConversion(id => id.Value, value => QuotaDefinitionId.From(value))
            .IsRequired();

        builder.Property(tq => tq.Limit).IsRequired();

        builder.Property(tq => tq.CreatedAtUtc).IsRequired();
        builder.Property(tq => tq.UpdatedAtUtc);
        builder.Property(tq => tq.IsDeleted).IsRequired();
        builder.Property(tq => tq.DeletedAtUtc);

        builder.MapXminConcurrencyToken(tq => tq.Version);
        builder.MapTenantScope("tenant_quotas");

        builder.HasIndex(tq => new { tq.TenantId, tq.QuotaDefinitionId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_quotas_tenant_quota");

        builder.Ignore(tq => tq.DomainEvents);
    }
}
