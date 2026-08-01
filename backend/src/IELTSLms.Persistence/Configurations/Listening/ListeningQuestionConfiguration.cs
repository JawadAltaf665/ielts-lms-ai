using IELTSLms.Domain.Listening;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Listening;

internal sealed class ListeningQuestionConfiguration : IEntityTypeConfiguration<ListeningQuestion>
{
    public void Configure(EntityTypeBuilder<ListeningQuestion> builder)
    {
        builder.ToTable("listening_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, value => ListeningQuestionId.From(value))
            .ValueGeneratedNever();

        builder.Property(q => q.TenantId);
        builder.Property(q => q.ListeningAudioId)
            .HasConversion(id => id.Value, value => ListeningAudioId.From(value))
            .IsRequired();

        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(q => q.Prompt).HasMaxLength(2000).IsRequired();

        // Structured JSON array of acceptable answers (ReadingAnswerGrader, reused) — never free text.
        builder.Property(q => q.AnswerKey).HasColumnType("jsonb").IsRequired();

        builder.Property(q => q.Sequence).IsRequired();

        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.Property(q => q.UpdatedAtUtc);
        builder.Property(q => q.IsDeleted).IsRequired();
        builder.Property(q => q.DeletedAtUtc);

        builder.MapXminConcurrencyToken(q => q.Version);

        builder.HasIndex(q => q.ListeningAudioId).HasDatabaseName("ix_listening_questions_audio_id");
        builder.HasIndex(q => new { q.ListeningAudioId, q.Sequence })
            .IsUnique()
            .HasDatabaseName("ix_listening_questions_audio_sequence_unique");

        builder.Ignore(q => q.DomainEvents);
    }
}
