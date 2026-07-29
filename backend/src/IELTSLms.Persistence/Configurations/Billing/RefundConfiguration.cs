using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => RefundId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.PaymentId)
            .HasConversion(id => id.Value, value => PaymentId.From(value))
            .IsRequired();

        builder.Property(r => r.AmountMinorUnits).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(r => r.StripeRefundId).HasMaxLength(128);

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);
        builder.MapTenantScope("refunds");

        builder.HasIndex(r => r.PaymentId).HasDatabaseName("ix_refunds_payment_id");
        builder.HasIndex(r => r.StripeRefundId).IsUnique().HasDatabaseName("ix_refunds_stripe_refund_id");

        builder.Ignore(r => r.DomainEvents);
    }
}
