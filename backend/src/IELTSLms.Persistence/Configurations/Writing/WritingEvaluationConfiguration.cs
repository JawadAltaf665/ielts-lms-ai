using IELTSLms.Domain.AI;
using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Writing;

internal sealed class WritingEvaluationConfiguration : IEntityTypeConfiguration<WritingEvaluation>
{
    public void Configure(EntityTypeBuilder<WritingEvaluation> builder)
    {
        builder.ToTable("writing_evaluations", t =>
        {
            // ERD ENT-029: criterion bands constrained to 0-9 in 0.5 steps.
            t.HasCheckConstraint("ck_writing_evaluations_task_response_range", BandRangeStepCheck("task_response"));
            t.HasCheckConstraint("ck_writing_evaluations_coherence_cohesion_range", BandRangeStepCheck("coherence_cohesion"));
            t.HasCheckConstraint("ck_writing_evaluations_lexical_resource_range", BandRangeStepCheck("lexical_resource"));
            t.HasCheckConstraint("ck_writing_evaluations_grammatical_range_accuracy_range", BandRangeStepCheck("grammatical_range_accuracy"));
            t.HasCheckConstraint("ck_writing_evaluations_confidence_range", "\"confidence\" >= 0 AND \"confidence\" <= 1");
            t.HasCheckConstraint(
                "ck_writing_evaluations_similarity_score_range", "\"similarity_score\" IS NULL OR (\"similarity_score\" >= 0 AND \"similarity_score\" <= 1)");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => WritingEvaluationId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.WritingSubmissionId)
            .HasConversion(id => id.Value, value => WritingSubmissionId.From(value))
            .IsRequired();

        builder.Property(e => e.TaskResponse).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.CoherenceCohesion).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.LexicalResource).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.GrammaticalRangeAccuracy).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.EstimatedBand).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.Confidence).HasColumnType("numeric(3,2)").IsRequired();
        builder.Property(e => e.FeedbackText).HasColumnType("text").IsRequired();
        builder.Property(e => e.SimilarityScore).HasColumnType("numeric(3,2)");
        builder.Property(e => e.RequiresHumanReview).IsRequired();

        builder.Property(e => e.AIResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));
        builder.Property(e => e.AIProviderId)
            .HasConversion(id => id!.Value.Value, value => AiProviderId.From(value));

        builder.Property(e => e.OverriddenTaskResponse).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenCoherenceCohesion).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenLexicalResource).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenGrammaticalRangeAccuracy).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenBand).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenBy);
        builder.Property(e => e.OverriddenAtUtc);
        builder.Property(e => e.OverrideReason).HasMaxLength(2000);

        builder.Property(e => e.ReviewedBy);
        builder.Property(e => e.ReviewedAtUtc);
        builder.Property(e => e.ReviewDecision).HasMaxLength(50);

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc);
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedAtUtc);

        builder.MapXminConcurrencyToken(e => e.Version);
        builder.MapTenantScope("writing_evaluations");

        // ERD: exactly one active evaluation per submission.
        builder.HasIndex(e => e.WritingSubmissionId).IsUnique().HasDatabaseName("ix_writing_evaluations_submission_id_unique");
        builder.HasIndex(e => new { e.TenantId, e.EstimatedBand }).HasDatabaseName("ix_writing_evaluations_tenant_band");

        // Review queue's filter shape (GetWritingReviewQueue): flagged, not yet reviewed or overridden.
        builder.HasIndex(e => new { e.TenantId, e.RequiresHumanReview, e.CreatedAtUtc })
            .HasDatabaseName("ix_writing_evaluations_tenant_requires_review");

        builder.Ignore(e => e.DomainEvents);
    }

    private static string BandRangeStepCheck(string column) =>
        "\"" + column + "\" >= 0 AND \"" + column + "\" <= 9 AND MOD(\"" + column + "\" * 2, 1) = 0";
}
