using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class AiPromptConfiguration : IEntityTypeConfiguration<AiPrompt>
{
    public void Configure(EntityTypeBuilder<AiPrompt> builder)
    {
        builder.ToTable("ai_prompts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => AiPromptId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global-default prompt row, same exemption as GrammarTopic.TenantId.
        builder.Property(p => p.TenantId);
        builder.Property(p => p.AiProviderId)
            .HasConversion(id => id!.Value.Value, value => AiProviderId.From(value));
        builder.Property(p => p.Feature).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PromptVersion).IsRequired();
        builder.Property(p => p.Scope).HasMaxLength(50).IsRequired();
        builder.Property(p => p.SystemTemplate).HasColumnType("text").IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_ai_prompts_tenant_id");

        builder.HasIndex(p => new { p.TenantId, p.Feature, p.PromptVersion, p.Scope })
            .IsUnique()
            .HasDatabaseName("ix_ai_prompts_tenant_feature_version_scope_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(p => new { p.Feature, p.PromptVersion, p.Scope })
            .IsUnique()
            .HasDatabaseName("ix_ai_prompts_global_feature_version_scope_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(p => p.DomainEvents);
    }
}
