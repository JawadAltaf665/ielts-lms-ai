using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => SubscriptionId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.BillingCustomerId)
            .HasConversion(id => id.Value, value => BillingCustomerId.From(value))
            .IsRequired();

        builder.Property(s => s.PlanId)
            .HasConversion(id => id.Value, value => PlanId.From(value))
            .IsRequired();

        builder.Property(s => s.PriceId)
            .HasConversion(id => id.Value, value => PriceId.From(value))
            .IsRequired();

        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(128);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(s => s.CurrentPeriodStart);
        builder.Property(s => s.CurrentPeriodEnd);
        builder.Property(s => s.TrialEndsAtUtc);
        builder.Property(s => s.CancelAtPeriodEnd).IsRequired();
        builder.Property(s => s.CancelledAtUtc);

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);
        builder.Property(s => s.IsDeleted).IsRequired();
        builder.Property(s => s.DeletedAtUtc);

        builder.MapXminConcurrencyToken(s => s.Version);
        builder.MapTenantScope("subscriptions");

        builder.HasIndex(s => s.TenantId).HasDatabaseName("ix_subscriptions_tenant_id_lookup");
        builder.HasIndex(s => s.Status).HasDatabaseName("ix_subscriptions_status");
        builder.HasIndex(s => s.StripeSubscriptionId).IsUnique().HasDatabaseName("ix_subscriptions_stripe_subscription_id");

        // One non-terminal subscription per tenant (13 §4: Expired is the only terminal state that
        // permits a new subscription row later, e.g. after resubscribing).
        builder.HasIndex(s => s.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_subscriptions_tenant_id_active_unique")
            .HasFilter("\"status\" <> 'Expired'");

        builder.Ignore(s => s.DomainEvents);
    }
}
