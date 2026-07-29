using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PaymentId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.InvoiceId)
            .HasConversion(id => id.Value, value => InvoiceId.From(value))
            .IsRequired();

        builder.Property(p => p.StripePaymentIntentId).HasMaxLength(128);
        builder.Property(p => p.AmountMinorUnits).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.PaidAtUtc);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("payments");

        builder.HasIndex(p => p.InvoiceId).HasDatabaseName("ix_payments_invoice_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_payments_status");
        builder.HasIndex(p => p.StripePaymentIntentId).IsUnique().HasDatabaseName("ix_payments_stripe_payment_intent_id");

        builder.Ignore(p => p.DomainEvents);
    }
}
