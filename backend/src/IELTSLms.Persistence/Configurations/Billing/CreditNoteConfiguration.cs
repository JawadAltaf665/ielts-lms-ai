using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("credit_notes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CreditNoteId.From(value))
            .ValueGeneratedNever();

        builder.Property(c => c.InvoiceId)
            .HasConversion(id => id.Value, value => InvoiceId.From(value))
            .IsRequired();

        builder.Property(c => c.AmountMinorUnits).IsRequired();
        builder.Property(c => c.Reason).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc);
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.DeletedAtUtc);

        builder.MapXminConcurrencyToken(c => c.Version);
        builder.MapTenantScope("credit_notes");

        builder.HasIndex(c => c.InvoiceId).HasDatabaseName("ix_credit_notes_invoice_id");

        builder.Ignore(c => c.DomainEvents);
    }
}
