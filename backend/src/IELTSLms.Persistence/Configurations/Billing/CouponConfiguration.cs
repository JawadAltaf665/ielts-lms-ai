using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CouponId.From(value))
            .ValueGeneratedNever();

        builder.Property(c => c.Code).HasMaxLength(Coupon.CodeMaxLength).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(Coupon.NameMaxLength).IsRequired();
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(c => c.Value).IsRequired();
        builder.Property(c => c.Currency).HasMaxLength(3);
        builder.Property(c => c.ValidFrom).IsRequired();
        builder.Property(c => c.ValidTo);
        builder.Property(c => c.MaxRedemptions);
        builder.Property(c => c.RedemptionCount).IsRequired();
        builder.Property(c => c.TenantId);
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.StripeCouponId).HasMaxLength(128);

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc);
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.DeletedAtUtc);

        builder.MapXminConcurrencyToken(c => c.Version);

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("ix_coupons_code");

        builder.Ignore(c => c.DomainEvents);
    }
}
