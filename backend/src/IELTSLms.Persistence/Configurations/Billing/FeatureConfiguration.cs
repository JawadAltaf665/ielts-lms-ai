using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("features");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FeatureId.From(value))
            .ValueGeneratedNever();

        builder.Property(f => f.Code).HasMaxLength(64).IsRequired();
        builder.Property(f => f.Name).HasMaxLength(256).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(1000);

        builder.Property(f => f.FeatureGroupId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? FeatureGroupId.From(value.Value) : (FeatureGroupId?)null);

        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.UpdatedAtUtc);
        builder.Property(f => f.IsDeleted).IsRequired();
        builder.Property(f => f.DeletedAtUtc);

        builder.MapXminConcurrencyToken(f => f.Version);

        builder.HasIndex(f => f.Code).IsUnique().HasDatabaseName("ix_features_code");

        builder.Ignore(f => f.DomainEvents);
    }
}
