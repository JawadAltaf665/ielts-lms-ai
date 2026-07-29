using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ProductId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.InternalCode).HasMaxLength(Product.CodeMaxLength).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.StripeProductId).HasMaxLength(128);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.InternalCode).IsUnique().HasDatabaseName("ix_products_internal_code");

        builder.Ignore(p => p.DomainEvents);
    }
}
