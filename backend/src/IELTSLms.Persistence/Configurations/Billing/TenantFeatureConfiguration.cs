using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.ToTable("tenant_features");

        builder.HasKey(tf => tf.Id);
        builder.Property(tf => tf.Id)
            .HasConversion(id => id.Value, value => TenantFeatureId.From(value))
            .ValueGeneratedNever();

        builder.Property(tf => tf.FeatureId)
            .HasConversion(id => id.Value, value => FeatureId.From(value))
            .IsRequired();

        builder.Property(tf => tf.IsEnabled).IsRequired();
        builder.Property(tf => tf.Reason).HasMaxLength(500);
        builder.Property(tf => tf.ExpiresAtUtc);

        builder.Property(tf => tf.CreatedAtUtc).IsRequired();
        builder.Property(tf => tf.UpdatedAtUtc);
        builder.Property(tf => tf.IsDeleted).IsRequired();
        builder.Property(tf => tf.DeletedAtUtc);

        builder.MapXminConcurrencyToken(tf => tf.Version);
        builder.MapTenantScope("tenant_features");

        builder.HasIndex(tf => new { tf.TenantId, tf.FeatureId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_features_tenant_feature");

        builder.Ignore(tf => tf.DomainEvents);
    }
}
