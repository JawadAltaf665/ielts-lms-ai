using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PlanQuotaConfiguration : IEntityTypeConfiguration<PlanQuota>
{
    public void Configure(EntityTypeBuilder<PlanQuota> builder)
    {
        builder.ToTable("plan_quotas");

        builder.HasKey(pq => pq.Id);
        builder.Property(pq => pq.Id)
            .HasConversion(id => id.Value, value => PlanQuotaId.From(value))
            .ValueGeneratedNever();

        builder.Property(pq => pq.PlanId)
            .HasConversion(id => id.Value, value => PlanId.From(value))
            .IsRequired();

        builder.Property(pq => pq.QuotaDefinitionId)
            .HasConversion(id => id.Value, value => QuotaDefinitionId.From(value))
            .IsRequired();

        builder.Property(pq => pq.Limit).IsRequired();

        builder.Property(pq => pq.CreatedAtUtc).IsRequired();
        builder.Property(pq => pq.UpdatedAtUtc);
        builder.Property(pq => pq.IsDeleted).IsRequired();
        builder.Property(pq => pq.DeletedAtUtc);

        builder.MapXminConcurrencyToken(pq => pq.Version);

        builder.HasIndex(pq => new { pq.PlanId, pq.QuotaDefinitionId })
            .IsUnique()
            .HasDatabaseName("ix_plan_quotas_plan_quota");

        builder.Ignore(pq => pq.DomainEvents);
    }
}
