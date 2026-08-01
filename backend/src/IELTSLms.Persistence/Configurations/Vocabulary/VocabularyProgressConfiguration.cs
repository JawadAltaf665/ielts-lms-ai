using IELTSLms.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Vocabulary;

internal sealed class VocabularyProgressConfiguration : IEntityTypeConfiguration<VocabularyProgress>
{
    public void Configure(EntityTypeBuilder<VocabularyProgress> builder)
    {
        builder.ToTable("vocabulary_progress");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => VocabularyProgressId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.VocabularyWordId)
            .HasConversion(id => id.Value, value => VocabularyWordId.From(value))
            .IsRequired();

        builder.Property(p => p.EaseFactor).HasColumnType("numeric(4,2)").IsRequired();
        builder.Property(p => p.IntervalDays).IsRequired();
        builder.Property(p => p.RepetitionCount).IsRequired();
        builder.Property(p => p.NextReviewAtUtc);
        builder.Property(p => p.LastGrade);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("vocabulary_progress");

        builder.HasIndex(p => new { p.TenantId, p.UserId, p.VocabularyWordId })
            .IsUnique()
            .HasDatabaseName("ix_vocabulary_progress_tenant_user_word_unique");

        // ERD ENT-018 calls out "next-review time" as an explicit index — the due-batch query's access path.
        builder.HasIndex(p => new { p.TenantId, p.UserId, p.NextReviewAtUtc })
            .HasDatabaseName("ix_vocabulary_progress_tenant_user_next_review");

        builder.Ignore(p => p.DomainEvents);
    }
}
