using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => InvoiceLineId.From(value))
            .ValueGeneratedNever();

        builder.Property(l => l.InvoiceId)
            .HasConversion(id => id.Value, value => InvoiceId.From(value))
            .IsRequired();

        builder.Property(l => l.PriceId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? PriceId.From(value.Value) : (PriceId?)null);

        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.Quantity).IsRequired();
        builder.Property(l => l.UnitAmountMinorUnits).IsRequired();

        builder.Property(l => l.CreatedAtUtc).IsRequired();
        builder.Property(l => l.UpdatedAtUtc);
        builder.Property(l => l.IsDeleted).IsRequired();
        builder.Property(l => l.DeletedAtUtc);

        builder.MapXminConcurrencyToken(l => l.Version);

        builder.HasIndex(l => l.InvoiceId).HasDatabaseName("ix_invoice_lines_invoice_id");

        builder.Ignore(l => l.DomainEvents);
        builder.Ignore(l => l.AmountMinorUnits);
    }
}
