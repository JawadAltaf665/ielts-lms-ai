using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Reading;

internal sealed class ReadingAttemptConfiguration : IEntityTypeConfiguration<ReadingAttempt>
{
    public void Configure(EntityTypeBuilder<ReadingAttempt> builder)
    {
        builder.ToTable("reading_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => ReadingAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.ReadingPassageId)
            .HasConversion(id => id.Value, value => ReadingPassageId.From(value))
            .IsRequired();

        // Left null this sprint; Sprint 8/9 populate when composing a passage into a session/exam.
        builder.Property(a => a.StudySessionId);
        builder.Property(a => a.WeeklyAssessmentId);
        builder.Property(a => a.MockExamId);

        builder.Property(a => a.StartedAtUtc).IsRequired();
        builder.Property(a => a.DeadlineAtUtc).IsRequired();
        builder.Property(a => a.SubmittedAtUtc);

        // Per-question results ({ questionId, submittedAnswer, isCorrect }) — same jsonb-string
        // convention as GrammarAttempt.AnswersJson; no separate child-answers table.
        builder.Property(a => a.AnswersJson).HasColumnType("jsonb");

        builder.Property(a => a.RawScore);
        builder.Property(a => a.MaxScore);
        builder.Property(a => a.Band).HasColumnType("numeric(2,1)");

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("reading_attempts");

        builder.HasIndex(a => new { a.TenantId, a.UserId }).HasDatabaseName("ix_reading_attempts_tenant_user");
        builder.HasIndex(a => a.ReadingPassageId).HasDatabaseName("ix_reading_attempts_passage_id");

        builder.Ignore(a => a.DomainEvents);
    }
}
