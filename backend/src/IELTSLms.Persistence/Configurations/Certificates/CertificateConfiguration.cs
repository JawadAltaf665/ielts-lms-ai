using IELTSLms.Domain.Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Certificates;

internal sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("certificates");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => CertificateId.From(value))
            .ValueGeneratedNever();

        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(c => c.SourceResultId).IsRequired();
        builder.Property(c => c.OverallBand).HasColumnType("numeric(2,1)").IsRequired();
        builder.Property(c => c.VerificationCode).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(c => c.IssuedAtUtc).IsRequired();
        builder.Property(c => c.RevokedAtUtc);
        builder.Property(c => c.RevokedReason).HasMaxLength(500);
        builder.Property(c => c.PdfStorageKey).HasMaxLength(500);

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc);
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.DeletedAtUtc);

        builder.MapXminConcurrencyToken(c => c.Version);
        builder.MapTenantScope("certificates");

        // VerificationCode is globally unique (ERD ENT-038) — verification is deliberately public/cross-tenant.
        builder.HasIndex(c => c.VerificationCode).IsUnique().HasDatabaseName("ix_certificates_verification_code_unique");
        builder.HasIndex(c => new { c.TenantId, c.UserId }).HasDatabaseName("ix_certificates_tenant_user");
        builder.HasIndex(c => new { c.TenantId, c.SourceType, c.SourceResultId })
            .HasDatabaseName("ix_certificates_tenant_source");

        builder.Ignore(c => c.DomainEvents);
    }
}
