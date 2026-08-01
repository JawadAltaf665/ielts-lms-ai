using IELTSLms.Domain.Assessment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Assessment;

internal sealed class MonthlyMockExamConfiguration : IEntityTypeConfiguration<MonthlyMockExam>
{
    public void Configure(EntityTypeBuilder<MonthlyMockExam> builder)
    {
        builder.ToTable("monthly_mock_exams");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => MockExamId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.State).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(e => e.StartedAtUtc).IsRequired();
        builder.Property(e => e.CompletedAtUtc);
        builder.Property(e => e.AbandonedAtUtc);

        builder.Property(e => e.ReadingAttemptId);
        builder.Property(e => e.ReadingStartedAtUtc);
        builder.Property(e => e.ReadingDeadlineAtUtc);
        builder.Property(e => e.ReadingCompletedAtUtc);

        builder.Property(e => e.ListeningAttemptId);
        builder.Property(e => e.ListeningStartedAtUtc);
        builder.Property(e => e.ListeningDeadlineAtUtc);
        builder.Property(e => e.ListeningCompletedAtUtc);

        builder.Property(e => e.WritingSubmissionId);
        builder.Property(e => e.WritingStartedAtUtc);
        builder.Property(e => e.WritingDeadlineAtUtc);
        builder.Property(e => e.WritingCompletedAtUtc);

        builder.Property(e => e.SpeakingAttemptId);
        builder.Property(e => e.SpeakingStartedAtUtc);
        builder.Property(e => e.SpeakingDeadlineAtUtc);
        builder.Property(e => e.SpeakingCompletedAtUtc);

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc);
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedAtUtc);

        builder.MapXminConcurrencyToken(e => e.Version);
        builder.MapTenantScope("monthly_mock_exams");

        // Single-active-session invariant (Sprint 8 business rule), enforced at the DB as the
        // backstop under a genuine race, in addition to the command-level pre-check
        // (StartMockExamCommandHandler) — a partial unique index so a user can have at most one
        // InProgress session at a time, while Completed/Abandoned history rows are unconstrained.
        builder.HasIndex(e => new { e.TenantId, e.UserId })
            .IsUnique()
            .HasFilter("\"state\" = 'InProgress'")
            .HasDatabaseName("ix_monthly_mock_exams_tenant_user_active");

        builder.HasIndex(e => e.ReadingAttemptId).HasDatabaseName("ix_monthly_mock_exams_reading_attempt_id");
        builder.HasIndex(e => e.ListeningAttemptId).HasDatabaseName("ix_monthly_mock_exams_listening_attempt_id");
        builder.HasIndex(e => e.WritingSubmissionId).HasDatabaseName("ix_monthly_mock_exams_writing_submission_id");
        builder.HasIndex(e => e.SpeakingAttemptId).HasDatabaseName("ix_monthly_mock_exams_speaking_attempt_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
