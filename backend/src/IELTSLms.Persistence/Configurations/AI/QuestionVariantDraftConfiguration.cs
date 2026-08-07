using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class QuestionVariantDraftConfiguration : IEntityTypeConfiguration<QuestionVariantDraft>
{
    public void Configure(EntityTypeBuilder<QuestionVariantDraft> builder)
    {
        builder.ToTable("question_variant_drafts");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => QuestionVariantDraftId.From(value))
            .ValueGeneratedNever();

        builder.Property(d => d.SourceType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(d => d.SourceQuestionId).IsRequired();

        // Structured JSON: { type, prompt, answerKey } — same shape a question is authored with.
        builder.Property(d => d.DraftContent).HasColumnType("jsonb").IsRequired();

        builder.Property(d => d.DifficultyDelta).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(d => d.AiResponseId)
            .HasConversion(id => id.Value, value => AiResponseId.From(value))
            .IsRequired();

        builder.Property(d => d.CreatedAtUtc).IsRequired();
        builder.Property(d => d.UpdatedAtUtc);
        builder.Property(d => d.IsDeleted).IsRequired();
        builder.Property(d => d.DeletedAtUtc);

        builder.MapXminConcurrencyToken(d => d.Version);
        builder.MapTenantScope("question_variant_drafts");

        builder.HasIndex(d => new { d.SourceType, d.SourceQuestionId })
            .HasDatabaseName("ix_question_variant_drafts_source");
        builder.HasIndex(d => new { d.TenantId, d.Status }).HasDatabaseName("ix_question_variant_drafts_tenant_status");

        builder.Ignore(d => d.DomainEvents);
    }
}
