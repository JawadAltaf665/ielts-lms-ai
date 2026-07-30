using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Grammar;

internal sealed class GrammarQuestionConfiguration : IEntityTypeConfiguration<GrammarQuestion>
{
    public void Configure(EntityTypeBuilder<GrammarQuestion> builder)
    {
        builder.ToTable("grammar_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, value => GrammarQuestionId.From(value))
            .ValueGeneratedNever();

        builder.Property(q => q.TenantId);
        builder.Property(q => q.GrammarExerciseId)
            .HasConversion(id => id.Value, value => GrammarExerciseId.From(value))
            .IsRequired();

        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(q => q.Prompt).HasMaxLength(2000).IsRequired();

        // Structured JSON array of acceptable answers (GrammarAnswerGrader) — never free text.
        builder.Property(q => q.AnswerKey).HasColumnType("jsonb").IsRequired();

        builder.Property(q => q.Sequence).IsRequired();

        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.Property(q => q.UpdatedAtUtc);
        builder.Property(q => q.IsDeleted).IsRequired();
        builder.Property(q => q.DeletedAtUtc);

        builder.MapXminConcurrencyToken(q => q.Version);

        builder.HasIndex(q => q.GrammarExerciseId).HasDatabaseName("ix_grammar_questions_exercise_id");
        builder.HasIndex(q => new { q.GrammarExerciseId, q.Sequence })
            .IsUnique()
            .HasDatabaseName("ix_grammar_questions_exercise_sequence_unique");

        builder.Ignore(q => q.DomainEvents);
    }
}
