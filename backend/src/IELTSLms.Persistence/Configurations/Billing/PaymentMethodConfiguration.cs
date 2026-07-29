using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => PaymentMethodId.From(value))
            .ValueGeneratedNever();

        builder.Property(m => m.BillingCustomerId)
            .HasConversion(id => id.Value, value => BillingCustomerId.From(value))
            .IsRequired();

        builder.Property(m => m.StripePaymentMethodId).HasMaxLength(128).IsRequired();
        builder.Property(m => m.Brand).HasMaxLength(32).IsRequired();
        builder.Property(m => m.Last4).HasMaxLength(4).IsRequired();
        builder.Property(m => m.ExpMonth).IsRequired();
        builder.Property(m => m.ExpYear).IsRequired();
        builder.Property(m => m.IsDefault).IsRequired();

        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.UpdatedAtUtc);
        builder.Property(m => m.IsDeleted).IsRequired();
        builder.Property(m => m.DeletedAtUtc);

        builder.MapXminConcurrencyToken(m => m.Version);
        builder.MapTenantScope("payment_methods");

        builder.HasIndex(m => m.BillingCustomerId).HasDatabaseName("ix_payment_methods_billing_customer_id");
        builder.HasIndex(m => m.StripePaymentMethodId).IsUnique().HasDatabaseName("ix_payment_methods_stripe_payment_method_id");

        builder.Ignore(m => m.DomainEvents);
    }
}
