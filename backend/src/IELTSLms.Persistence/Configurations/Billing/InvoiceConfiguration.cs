using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => InvoiceId.From(value))
            .ValueGeneratedNever();

        builder.Property(i => i.SubscriptionId)
            .HasConversion(id => id.Value, value => SubscriptionId.From(value))
            .IsRequired();

        builder.Property(i => i.BillingCustomerId)
            .HasConversion(id => id.Value, value => BillingCustomerId.From(value))
            .IsRequired();

        builder.Property(i => i.StripeInvoiceId).HasMaxLength(128);
        builder.Property(i => i.Number).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
        builder.Property(i => i.SubtotalMinorUnits).IsRequired();
        builder.Property(i => i.TaxMinorUnits).IsRequired();
        builder.Property(i => i.TotalMinorUnits).IsRequired();
        builder.Property(i => i.IssuedAtUtc);
        builder.Property(i => i.DueAtUtc);
        builder.Property(i => i.PaidAtUtc);

        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.UpdatedAtUtc);
        builder.Property(i => i.IsDeleted).IsRequired();
        builder.Property(i => i.DeletedAtUtc);

        builder.MapXminConcurrencyToken(i => i.Version);
        builder.MapTenantScope("invoices");

        builder.HasIndex(i => i.Number).IsUnique().HasDatabaseName("ix_invoices_number");
        builder.HasIndex(i => i.Status).HasDatabaseName("ix_invoices_status");
        builder.HasIndex(i => i.StripeInvoiceId).IsUnique().HasDatabaseName("ix_invoices_stripe_invoice_id");

        builder.Ignore(i => i.DomainEvents);
    }
}
