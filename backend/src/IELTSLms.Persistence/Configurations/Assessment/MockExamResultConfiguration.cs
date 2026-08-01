using IELTSLms.Domain.Assessment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Assessment;

internal sealed class MockExamResultConfiguration : IEntityTypeConfiguration<MockExamResult>
{
    public void Configure(EntityTypeBuilder<MockExamResult> builder)
    {
        builder.ToTable("mock_exam_results");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => MockExamResultId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.MockExamId)
            .HasConversion(id => id.Value, value => MockExamId.From(value))
            .IsRequired();

        builder.Property(r => r.ReadingBand).HasColumnType("numeric(2,1)");
        builder.Property(r => r.ListeningBand).HasColumnType("numeric(2,1)");
        builder.Property(r => r.WritingBand).HasColumnType("numeric(2,1)");
        builder.Property(r => r.SpeakingBand).HasColumnType("numeric(2,1)");
        builder.Property(r => r.OverallBand).HasColumnType("numeric(2,1)");
        builder.Property(r => r.IsFinalized).IsRequired();
        builder.Property(r => r.FinalizedAtUtc);

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);
        builder.MapTenantScope("mock_exam_results");

        // 1:1 with MonthlyMockExam (ERD ENT-037).
        builder.HasIndex(r => r.MockExamId).IsUnique().HasDatabaseName("ix_mock_exam_results_mock_exam_id_unique");

        builder.Ignore(r => r.DomainEvents);
    }
}
