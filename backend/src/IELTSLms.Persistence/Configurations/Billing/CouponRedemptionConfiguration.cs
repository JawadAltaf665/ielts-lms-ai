using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("coupon_redemptions");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => CouponRedemptionId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.CouponId)
            .HasConversion(id => id.Value, value => CouponId.From(value))
            .IsRequired();

        builder.Property(r => r.SubscriptionId)
            .HasConversion(id => id.Value, value => SubscriptionId.From(value))
            .IsRequired();

        builder.Property(r => r.IsActive).IsRequired();

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);
        builder.MapTenantScope("coupon_redemptions");

        builder.HasIndex(r => r.CouponId).HasDatabaseName("ix_coupon_redemptions_coupon_id");

        // One active redemption per subscription (13 §7 — stacking disallowed by default). A partial
        // index (not a plain unique constraint) so a revoked/inactive redemption doesn't block reuse.
        builder.HasIndex(r => r.SubscriptionId)
            .IsUnique()
            .HasDatabaseName("ix_coupon_redemptions_subscription_active_unique")
            .HasFilter("\"is_active\" = true");

        builder.Ignore(r => r.DomainEvents);
    }
}
