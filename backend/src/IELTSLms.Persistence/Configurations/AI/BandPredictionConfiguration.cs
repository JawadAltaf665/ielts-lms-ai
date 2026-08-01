using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class BandPredictionConfiguration : IEntityTypeConfiguration<BandPrediction>
{
    public void Configure(EntityTypeBuilder<BandPrediction> builder)
    {
        builder.ToTable("band_predictions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => BandPredictionId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.AiOperationId)
            .HasConversion(id => id.Value, value => AiOperationId.From(value))
            .IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(p => p.OverallBand).HasColumnType("numeric(2,1)");
        builder.Property(p => p.ReadingBand).HasColumnType("numeric(2,1)");
        builder.Property(p => p.ListeningBand).HasColumnType("numeric(2,1)");
        builder.Property(p => p.WritingBand).HasColumnType("numeric(2,1)");
        builder.Property(p => p.SpeakingBand).HasColumnType("numeric(2,1)");
        builder.Property(p => p.Confidence).HasColumnType("numeric(3,2)");
        builder.Property(p => p.RecommendMorePractice).IsRequired();

        builder.Property(p => p.AIResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));
        builder.Property(p => p.ErrorCode).HasMaxLength(100);
        builder.Property(p => p.ErrorMessage).HasMaxLength(1000);
        builder.Property(p => p.CompletedAtUtc);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("band_predictions");

        builder.HasIndex(p => new { p.TenantId, p.UserId }).HasDatabaseName("ix_band_predictions_tenant_user");

        builder.Ignore(p => p.DomainEvents);
    }
}
