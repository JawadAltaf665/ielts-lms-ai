using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("prices");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.PlanId)
            .HasConversion(id => id.Value, value => PlanId.From(value))
            .IsRequired();

        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.AmountMinorUnits).HasColumnName("unit_amount_minor_units").IsRequired();
            amount.Property(a => a.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(p => p.Amount).IsRequired();

        builder.Property(p => p.Interval).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.StripePriceId).HasMaxLength(128);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => new { p.PlanId, p.Interval }).HasDatabaseName("ix_prices_plan_interval");
        builder.HasIndex(p => p.StripePriceId).IsUnique().HasDatabaseName("ix_prices_stripe_price_id");

        builder.Ignore(p => p.DomainEvents);
    }
}
