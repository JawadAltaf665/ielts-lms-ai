using IELTSLms.Domain.AI;
using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Speaking;

internal sealed class SpeakingEvaluationConfiguration : IEntityTypeConfiguration<SpeakingEvaluation>
{
    public void Configure(EntityTypeBuilder<SpeakingEvaluation> builder)
    {
        builder.ToTable("speaking_evaluations", t =>
        {
            // ERD ENT-032: criterion bands constrained to 0-9 in 0.5 steps.
            t.HasCheckConstraint("ck_speaking_evaluations_fluency_coherence_range", BandRangeStepCheck("fluency_coherence"));
            t.HasCheckConstraint("ck_speaking_evaluations_lexical_resource_range", BandRangeStepCheck("lexical_resource"));
            t.HasCheckConstraint("ck_speaking_evaluations_grammatical_range_accuracy_range", BandRangeStepCheck("grammatical_range_accuracy"));
            t.HasCheckConstraint("ck_speaking_evaluations_pronunciation_range", BandRangeStepCheck("pronunciation"));
            t.HasCheckConstraint("ck_speaking_evaluations_confidence_range", "\"confidence\" >= 0 AND \"confidence\" <= 1");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => SpeakingEvaluationId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.SpeakingAttemptId)
            .HasConversion(id => id.Value, value => SpeakingAttemptId.From(value))
            .IsRequired();

        builder.Property(e => e.FluencyCoherence).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.LexicalResource).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.GrammaticalRangeAccuracy).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.Pronunciation).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.EstimatedBand).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(e => e.Confidence).HasColumnType("numeric(3,2)").IsRequired();
        builder.Property(e => e.FeedbackText).HasColumnType("text").IsRequired();
        builder.Property(e => e.RequiresHumanReview).IsRequired();

        builder.Property(e => e.AIResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));
        builder.Property(e => e.AIProviderId)
            .HasConversion(id => id!.Value.Value, value => AiProviderId.From(value));

        builder.Property(e => e.OverriddenFluencyCoherence).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenLexicalResource).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenGrammaticalRangeAccuracy).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenPronunciation).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenBand).HasColumnType("numeric(2,1)");
        builder.Property(e => e.OverriddenBy);
        builder.Property(e => e.OverriddenAtUtc);
        builder.Property(e => e.OverrideReason).HasMaxLength(2000);

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc);
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedAtUtc);

        builder.MapXminConcurrencyToken(e => e.Version);
        builder.MapTenantScope("speaking_evaluations");

        // ERD: exactly one active evaluation per attempt.
        builder.HasIndex(e => e.SpeakingAttemptId).IsUnique().HasDatabaseName("ix_speaking_evaluations_attempt_id_unique");
        builder.HasIndex(e => new { e.TenantId, e.EstimatedBand }).HasDatabaseName("ix_speaking_evaluations_tenant_band");

        builder.Ignore(e => e.DomainEvents);
    }

    private static string BandRangeStepCheck(string column) =>
        "\"" + column + "\" >= 0 AND \"" + column + "\" <= 9 AND MOD(\"" + column + "\" * 2, 1) = 0";
}
