using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PlanId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.ProductId)
            .HasConversion(id => id.Value, value => ProductId.From(value))
            .IsRequired();

        builder.Property(p => p.Code).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(Plan.NameMaxLength).IsRequired();
        builder.Property(p => p.DisplayOrder).IsRequired();
        builder.Property(p => p.TrialEligible).IsRequired();
        builder.Property(p => p.TrialDays).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("ix_plans_code");
        builder.HasIndex(p => p.DisplayOrder).HasDatabaseName("ix_plans_display_order");

        builder.Ignore(p => p.DomainEvents);
    }
}
