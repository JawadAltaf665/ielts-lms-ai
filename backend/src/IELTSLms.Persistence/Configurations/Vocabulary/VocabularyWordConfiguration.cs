using IELTSLms.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Vocabulary;

internal sealed class VocabularyWordConfiguration : IEntityTypeConfiguration<VocabularyWord>
{
    public void Configure(EntityTypeBuilder<VocabularyWord> builder)
    {
        builder.ToTable("vocabulary_words");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => VocabularyWordId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog word, visible read-only to every tenant.
        builder.Property(w => w.TenantId);
        builder.Property(w => w.Word).HasMaxLength(100).IsRequired();
        builder.Property(w => w.Sense).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Meaning).HasMaxLength(2000).IsRequired();
        builder.Property(w => w.ExamplesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(w => w.FrequencyBand).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(w => w.Topic).HasMaxLength(100);
        builder.Property(w => w.CategoryId);
        builder.Property(w => w.DifficultyLevel).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(w => w.CreatedAtUtc).IsRequired();
        builder.Property(w => w.UpdatedAtUtc);
        builder.Property(w => w.IsDeleted).IsRequired();
        builder.Property(w => w.DeletedAtUtc);

        builder.MapXminConcurrencyToken(w => w.Version);

        builder.HasIndex(w => w.TenantId).HasDatabaseName("ix_vocabulary_words_tenant_id");
        builder.HasIndex(w => w.Word).HasDatabaseName("ix_vocabulary_words_word");
        builder.HasIndex(w => w.Topic).HasDatabaseName("ix_vocabulary_words_topic");

        // (TenantId, Word, Sense) uniqueness per catalog spec (ERD ENT-017): within a tenant's own
        // words, and separately among global words. Two partial indexes because Postgres treats every
        // NULL tenant_id as distinct, so a single (tenant_id, word, sense) unique index would not
        // constrain global rows at all — mirrors GrammarTopicConfiguration's title-uniqueness split.
        builder.HasIndex(w => new { w.TenantId, w.Word, w.Sense })
            .IsUnique()
            .HasDatabaseName("ix_vocabulary_words_tenant_word_sense_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(w => new { w.Word, w.Sense })
            .IsUnique()
            .HasDatabaseName("ix_vocabulary_words_global_word_sense_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(w => w.DomainEvents);
    }
}
