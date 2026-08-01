using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Writing;

internal sealed class WritingSubmissionConfiguration : IEntityTypeConfiguration<WritingSubmission>
{
    public void Configure(EntityTypeBuilder<WritingSubmission> builder)
    {
        builder.ToTable("writing_submissions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => WritingSubmissionId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.WritingTaskId)
            .HasConversion(id => id.Value, value => WritingTaskId.From(value))
            .IsRequired();
        builder.Property(s => s.ResponseText).HasColumnType("text").IsRequired();
        builder.Property(s => s.WordCount).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(s => s.SubmittedAtUtc);
        builder.Property(s => s.WritingEvaluationId)
            .HasConversion(id => id!.Value.Value, value => WritingEvaluationId.From(value));

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);
        builder.Property(s => s.IsDeleted).IsRequired();
        builder.Property(s => s.DeletedAtUtc);

        builder.MapXminConcurrencyToken(s => s.Version);
        builder.MapTenantScope("writing_submissions");

        builder.HasIndex(s => new { s.TenantId, s.UserId }).HasDatabaseName("ix_writing_submissions_tenant_user");
        builder.HasIndex(s => s.WritingTaskId).HasDatabaseName("ix_writing_submissions_writing_task_id");
        builder.HasIndex(s => s.Status).HasDatabaseName("ix_writing_submissions_status");

        builder.Ignore(s => s.DomainEvents);
    }
}
