using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("plan_features");

        builder.HasKey(pf => pf.Id);
        builder.Property(pf => pf.Id)
            .HasConversion(id => id.Value, value => PlanFeatureId.From(value))
            .ValueGeneratedNever();

        builder.Property(pf => pf.PlanId)
            .HasConversion(id => id.Value, value => PlanId.From(value))
            .IsRequired();

        builder.Property(pf => pf.FeatureId)
            .HasConversion(id => id.Value, value => FeatureId.From(value))
            .IsRequired();

        builder.Property(pf => pf.IsEnabled).IsRequired();

        builder.Property(pf => pf.CreatedAtUtc).IsRequired();
        builder.Property(pf => pf.UpdatedAtUtc);
        builder.Property(pf => pf.IsDeleted).IsRequired();
        builder.Property(pf => pf.DeletedAtUtc);

        builder.MapXminConcurrencyToken(pf => pf.Version);

        builder.HasIndex(pf => new { pf.PlanId, pf.FeatureId })
            .IsUnique()
            .HasDatabaseName("ix_plan_features_plan_feature");

        builder.Ignore(pf => pf.DomainEvents);
    }
}
