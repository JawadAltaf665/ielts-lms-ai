using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Reading;

internal sealed class ReadingQuestionConfiguration : IEntityTypeConfiguration<ReadingQuestion>
{
    public void Configure(EntityTypeBuilder<ReadingQuestion> builder)
    {
        builder.ToTable("reading_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, value => ReadingQuestionId.From(value))
            .ValueGeneratedNever();

        builder.Property(q => q.TenantId);
        builder.Property(q => q.ReadingPassageId)
            .HasConversion(id => id.Value, value => ReadingPassageId.From(value))
            .IsRequired();

        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(q => q.Prompt).HasMaxLength(2000).IsRequired();

        // Structured JSON array of acceptable answers (ReadingAnswerGrader) — never free text.
        builder.Property(q => q.AnswerKey).HasColumnType("jsonb").IsRequired();

        builder.Property(q => q.Sequence).IsRequired();

        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.Property(q => q.UpdatedAtUtc);
        builder.Property(q => q.IsDeleted).IsRequired();
        builder.Property(q => q.DeletedAtUtc);

        builder.MapXminConcurrencyToken(q => q.Version);

        builder.HasIndex(q => q.ReadingPassageId).HasDatabaseName("ix_reading_questions_passage_id");
        builder.HasIndex(q => new { q.ReadingPassageId, q.Sequence })
            .IsUnique()
            .HasDatabaseName("ix_reading_questions_passage_sequence_unique");

        builder.Ignore(q => q.DomainEvents);
    }
}
