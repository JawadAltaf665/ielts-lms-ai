using IELTSLms.Domain.AI;
using IELTSLms.Domain.SentenceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.SentenceBuilder;

internal sealed class SentenceAttemptConfiguration : IEntityTypeConfiguration<SentenceAttempt>
{
    public void Configure(EntityTypeBuilder<SentenceAttempt> builder)
    {
        builder.ToTable("sentence_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => SentenceAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.SentenceExerciseId)
            .HasConversion(id => id.Value, value => SentenceExerciseId.From(value))
            .IsRequired();

        builder.Property(a => a.SubmittedText).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.Score);

        builder.Property(a => a.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));
        builder.Property(a => a.Feedback).HasMaxLength(4000);

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);
        builder.MapTenantScope("sentence_attempts");

        builder.HasIndex(a => new { a.TenantId, a.UserId }).HasDatabaseName("ix_sentence_attempts_tenant_user");
        builder.HasIndex(a => a.SentenceExerciseId).HasDatabaseName("ix_sentence_attempts_exercise_id");

        builder.Ignore(a => a.DomainEvents);
    }
}
