using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class ProcessedWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("processed_webhook_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(128).ValueGeneratedNever();

        builder.Property(e => e.ProcessedAtUtc).IsRequired();
    }
}
