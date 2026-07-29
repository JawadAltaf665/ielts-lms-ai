using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class SubscriptionItemConfiguration : IEntityTypeConfiguration<SubscriptionItem>
{
    public void Configure(EntityTypeBuilder<SubscriptionItem> builder)
    {
        builder.ToTable("subscription_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => SubscriptionItemId.From(value))
            .ValueGeneratedNever();

        builder.Property(i => i.SubscriptionId)
            .HasConversion(id => id.Value, value => SubscriptionId.From(value))
            .IsRequired();

        builder.Property(i => i.PriceId)
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .IsRequired();

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.StripeSubscriptionItemId).HasMaxLength(128);

        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.UpdatedAtUtc);
        builder.Property(i => i.IsDeleted).IsRequired();
        builder.Property(i => i.DeletedAtUtc);

        builder.MapXminConcurrencyToken(i => i.Version);

        builder.HasIndex(i => i.SubscriptionId).HasDatabaseName("ix_subscription_items_subscription_id");

        builder.Ignore(i => i.DomainEvents);
    }
}
