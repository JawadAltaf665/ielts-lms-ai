using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class FeatureGroupConfiguration : IEntityTypeConfiguration<FeatureGroup>
{
    public void Configure(EntityTypeBuilder<FeatureGroup> builder)
    {
        builder.ToTable("feature_groups");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, value => FeatureGroupId.From(value))
            .ValueGeneratedNever();

        builder.Property(g => g.Code).HasMaxLength(64).IsRequired();
        builder.Property(g => g.Name).HasMaxLength(256).IsRequired();
        builder.Property(g => g.DisplayOrder).IsRequired();

        builder.Property(g => g.CreatedAtUtc).IsRequired();
        builder.Property(g => g.UpdatedAtUtc);
        builder.Property(g => g.IsDeleted).IsRequired();
        builder.Property(g => g.DeletedAtUtc);

        builder.MapXminConcurrencyToken(g => g.Version);

        builder.HasIndex(g => g.Code).IsUnique().HasDatabaseName("ix_feature_groups_code");

        builder.Ignore(g => g.DomainEvents);
    }
}
