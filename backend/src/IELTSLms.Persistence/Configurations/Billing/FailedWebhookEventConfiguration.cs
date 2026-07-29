using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class FailedWebhookEventConfiguration : IEntityTypeConfiguration<FailedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<FailedWebhookEvent> builder)
    {
        builder.ToTable("failed_webhook_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(128).ValueGeneratedNever();

        builder.Property(e => e.EventType).HasMaxLength(128).IsRequired();
        builder.Property(e => e.RawJson).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.LastError).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.RetryCount).IsRequired();
        builder.Property(e => e.FailedAtUtc).IsRequired();
        builder.Property(e => e.LastRetryAtUtc);

        builder.HasIndex(e => e.RetryCount).HasDatabaseName("ix_failed_webhook_events_retry_count");
    }
}
