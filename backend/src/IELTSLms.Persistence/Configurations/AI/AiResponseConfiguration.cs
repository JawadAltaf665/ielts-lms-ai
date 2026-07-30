using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class AiResponseConfiguration : IEntityTypeConfiguration<AiResponse>
{
    public void Configure(EntityTypeBuilder<AiResponse> builder)
    {
        builder.ToTable("ai_responses");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => AiResponseId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.AiProviderId)
            .HasConversion(id => id.Value, value => AiProviderId.From(value))
            .IsRequired();
        builder.Property(r => r.AiPromptId)
            .HasConversion(id => id!.Value.Value, value => AiPromptId.From(value));
        builder.Property(r => r.PromptVersion).IsRequired();
        builder.Property(r => r.RequestingEntityType).HasMaxLength(100).IsRequired();
        builder.Property(r => r.RequestingEntityId).IsRequired();
        builder.Property(r => r.InputHash).HasMaxLength(64).IsRequired();
        builder.Property(r => r.Model).HasMaxLength(100).IsRequired();
        builder.Property(r => r.PromptTokens).IsRequired();
        builder.Property(r => r.CompletionTokens).IsRequired();
        builder.Property(r => r.LatencyMs).IsRequired();
        builder.Property(r => r.CostMinorUnits).IsRequired();
        builder.Property(r => r.CostCurrency).HasMaxLength(3).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(r => r.OutputJson).HasColumnType("jsonb");
        builder.Property(r => r.ProviderResponseId).HasMaxLength(200);

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);
        builder.MapTenantScope("ai_responses");

        // Leading TenantId + CreatedAt per ERD §10.2 partitioning guidance — a partition candidate,
        // not partitioned yet, but the index shape doesn't fight that future.
        builder.HasIndex(r => new { r.TenantId, r.CreatedAtUtc }).HasDatabaseName("ix_ai_responses_tenant_created_at");
        builder.HasIndex(r => r.ProviderResponseId)
            .IsUnique()
            .HasDatabaseName("ix_ai_responses_provider_response_id_unique")
            .HasFilter("\"provider_response_id\" IS NOT NULL");

        builder.Ignore(r => r.DomainEvents);
    }
}
