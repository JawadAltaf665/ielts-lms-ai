using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider>
{
    public void Configure(EntityTypeBuilder<AiProvider> builder)
    {
        builder.ToTable("ai_providers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => AiProviderId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global-catalog provider row, same exemption as GrammarTopic.TenantId.
        builder.Property(p => p.TenantId);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(p => p.BaseUrl).HasMaxLength(500).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_ai_providers_tenant_id");

        builder.HasIndex(p => new { p.TenantId, p.Name })
            .IsUnique()
            .HasDatabaseName("ix_ai_providers_tenant_name_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("ix_ai_providers_global_name_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(p => p.DomainEvents);
    }
}
