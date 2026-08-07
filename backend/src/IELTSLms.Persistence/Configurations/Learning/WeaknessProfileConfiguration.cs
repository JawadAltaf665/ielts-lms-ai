using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Learning;

internal sealed class WeaknessProfileConfiguration : IEntityTypeConfiguration<WeaknessProfile>
{
    public void Configure(EntityTypeBuilder<WeaknessProfile> builder)
    {
        builder.ToTable("weakness_profiles");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => WeaknessProfileId.From(value))
            .ValueGeneratedNever();

        builder.Property(w => w.UserId).IsRequired();

        // Structured JSON: [{ skill, score, sampleSize }] — see SkillWeaknessScoreDto (Application).
        builder.Property(w => w.SkillScoresJson).HasColumnType("jsonb").IsRequired();

        builder.Property(w => w.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));

        builder.Property(w => w.ComputedAtUtc).IsRequired();

        builder.Property(w => w.CreatedAtUtc).IsRequired();
        builder.Property(w => w.UpdatedAtUtc);
        builder.Property(w => w.IsDeleted).IsRequired();
        builder.Property(w => w.DeletedAtUtc);

        builder.MapXminConcurrencyToken(w => w.Version);
        builder.MapTenantScope("weakness_profiles");

        builder.HasIndex(w => new { w.TenantId, w.UserId })
            .IsUnique()
            .HasDatabaseName("ix_weakness_profiles_tenant_user_unique");

        builder.Ignore(w => w.DomainEvents);
    }
}
