using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class BillingCustomerConfiguration : IEntityTypeConfiguration<BillingCustomer>
{
    public void Configure(EntityTypeBuilder<BillingCustomer> builder)
    {
        builder.ToTable("billing_customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => BillingCustomerId.From(value))
            .ValueGeneratedNever();

        builder.Property(c => c.Email).HasMaxLength(320).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(32);
        builder.Property(c => c.StripeCustomerId).HasMaxLength(128);
        builder.Property(c => c.TaxExempt).IsRequired();

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Line1).HasColumnName("address_line1").HasMaxLength(256);
            address.Property(a => a.Line2).HasColumnName("address_line2").HasMaxLength(256);
            address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(128);
            address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(128);
            address.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(32);
            address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(2);
        });

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc);
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.DeletedAtUtc);

        builder.MapXminConcurrencyToken(c => c.Version);
        builder.MapTenantScope("billing_customers");

        builder.HasIndex(c => c.TenantId).IsUnique().HasDatabaseName("ix_billing_customers_tenant_id_unique");
        builder.HasIndex(c => c.StripeCustomerId).IsUnique().HasDatabaseName("ix_billing_customers_stripe_customer_id");

        builder.Ignore(c => c.DomainEvents);
    }
}
