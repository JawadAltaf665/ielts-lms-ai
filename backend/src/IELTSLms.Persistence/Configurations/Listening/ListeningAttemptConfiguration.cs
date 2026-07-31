using IELTSLms.Domain.Listening;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Listening;

internal sealed class ListeningAttemptConfiguration : IEntityTypeConfiguration<ListeningAttempt>
{
    public void Configure(EntityTypeBuilder<ListeningAttempt> builder)
    {
        builder.ToTable("listening_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => ListeningAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.ListeningAudioId)
            .HasConversion(id => id.Value, value => ListeningAudioId.From(value))
            .IsRequired();

        builder.Property(a => a.Mode).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(a => a.StartedAtUtc).IsRequired();
        builder.Property(a => a.DeadlineAtUtc).IsRequired();
        builder.Property(a => a.SubmittedAtUtc);

        // Per-question results ({ questionId, submittedAnswer, isCorrect }) — same jsonb-string
        // convention as ReadingAttempt.AnswersJson; no separate child-answers table.
        builder.Property(a => a.AnswersJson).HasColumnType("jsonb");

        builder.Property(a => a.RawScore);
        builder.Property(a => a.MaxScore);
        builder.Property(a => a.Band).HasColumnType("numeric(2,1)");

        builder.Property(a => a.ReplayCount).IsRequired();

        // Timestamped play/replay audit trail (Sprint 5 business rule) — jsonb-string, same convention.
        builder.Property(a => a.PlaybackEventsJson).HasColumnType("jsonb").IsRequired();

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("listening_attempts");

        builder.HasIndex(a => new { a.TenantId, a.UserId }).HasDatabaseName("ix_listening_attempts_tenant_user");
        builder.HasIndex(a => a.ListeningAudioId).HasDatabaseName("ix_listening_attempts_audio_id");

        builder.Ignore(a => a.DomainEvents);
    }
}
