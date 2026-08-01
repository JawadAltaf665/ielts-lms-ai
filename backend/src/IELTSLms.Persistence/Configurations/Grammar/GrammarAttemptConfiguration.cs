using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Grammar;

internal sealed class GrammarAttemptConfiguration : IEntityTypeConfiguration<GrammarAttempt>
{
    public void Configure(EntityTypeBuilder<GrammarAttempt> builder)
    {
        builder.ToTable("grammar_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => GrammarAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.GrammarExerciseId)
            .HasConversion(id => id.Value, value => GrammarExerciseId.From(value))
            .IsRequired();

        // Per-question results ({ questionId, submittedAnswer, isCorrect }) — same jsonb-string
        // convention as FailedWebhookEvent.RawJson; no separate child-answers table.
        builder.Property(a => a.AnswersJson).HasColumnType("jsonb").IsRequired();

        builder.Property(a => a.RawScore).IsRequired();
        builder.Property(a => a.MaxScore).IsRequired();

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("grammar_attempts");

        builder.HasIndex(a => new { a.TenantId, a.UserId }).HasDatabaseName("ix_grammar_attempts_tenant_user");
        builder.HasIndex(a => a.GrammarExerciseId).HasDatabaseName("ix_grammar_attempts_exercise_id");

        builder.Ignore(a => a.DomainEvents);
    }
}
