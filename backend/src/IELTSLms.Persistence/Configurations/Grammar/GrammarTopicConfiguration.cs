using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Grammar;

internal sealed class GrammarTopicConfiguration : IEntityTypeConfiguration<GrammarTopic>
{
    public void Configure(EntityTypeBuilder<GrammarTopic> builder)
    {
        builder.ToTable("grammar_topics");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => GrammarTopicId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog topic, visible read-only to every tenant.
        builder.Property(t => t.TenantId);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000).IsRequired();
        builder.Property(t => t.Track).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(t => t.Difficulty).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(t => t.CategoryId);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.DeletedAtUtc);

        builder.MapXminConcurrencyToken(t => t.Version);

        builder.HasIndex(t => t.TenantId).HasDatabaseName("ix_grammar_topics_tenant_id");

        // Title uniqueness per catalog spec (ERD ENT-016): within a tenant's own topics, and
        // separately among global topics. Two partial indexes because Postgres treats every NULL
        // tenant_id as distinct, so a single (tenant_id, title) unique index would not constrain
        // global rows at all.
        builder.HasIndex(t => new { t.TenantId, t.Title })
            .IsUnique()
            .HasDatabaseName("ix_grammar_topics_tenant_title_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(t => t.Title)
            .IsUnique()
            .HasDatabaseName("ix_grammar_topics_global_title_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(t => t.DomainEvents);
    }
}
