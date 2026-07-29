using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Billing;

internal sealed class QuotaDefinitionConfiguration : IEntityTypeConfiguration<QuotaDefinition>
{
    public void Configure(EntityTypeBuilder<QuotaDefinition> builder)
    {
        builder.ToTable("quota_definitions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, value => QuotaDefinitionId.From(value))
            .ValueGeneratedNever();

        builder.Property(q => q.Code).HasMaxLength(64).IsRequired();
        builder.Property(q => q.Name).HasMaxLength(256).IsRequired();
        builder.Property(q => q.Unit).HasMaxLength(32).IsRequired();
        builder.Property(q => q.ResetCycle).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.Property(q => q.UpdatedAtUtc);
        builder.Property(q => q.IsDeleted).IsRequired();
        builder.Property(q => q.DeletedAtUtc);

        builder.MapXminConcurrencyToken(q => q.Version);

        builder.HasIndex(q => q.Code).IsUnique().HasDatabaseName("ix_quota_definitions_code");

        builder.Ignore(q => q.DomainEvents);
    }
}
