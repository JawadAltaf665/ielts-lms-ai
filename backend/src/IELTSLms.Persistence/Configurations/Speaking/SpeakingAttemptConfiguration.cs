using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Speaking;

internal sealed class SpeakingAttemptConfiguration : IEntityTypeConfiguration<SpeakingAttempt>
{
    public void Configure(EntityTypeBuilder<SpeakingAttempt> builder)
    {
        builder.ToTable("speaking_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => SpeakingAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.SpeakingTaskId)
            .HasConversion(id => id.Value, value => SpeakingTaskId.From(value))
            .IsRequired();
        builder.Property(a => a.FileId)
            .HasConversion(id => id.Value, value => FileAssetId.From(value))
            .IsRequired();
        builder.Property(a => a.DurationSeconds).IsRequired();
        builder.Property(a => a.TranscriptText).HasColumnType("text");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(a => a.SubmittedAtUtc);
        builder.Property(a => a.SpeakingEvaluationId)
            .HasConversion(id => id!.Value.Value, value => SpeakingEvaluationId.From(value));

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("speaking_attempts");

        builder.HasIndex(a => new { a.TenantId, a.UserId }).HasDatabaseName("ix_speaking_attempts_tenant_user");
        builder.HasIndex(a => a.SpeakingTaskId).HasDatabaseName("ix_speaking_attempts_speaking_task_id");
        builder.HasIndex(a => a.Status).HasDatabaseName("ix_speaking_attempts_status");

        builder.Ignore(a => a.DomainEvents);
    }
}
