using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class AiOperationConfiguration : IEntityTypeConfiguration<AiOperation>
{
    public void Configure(EntityTypeBuilder<AiOperation> builder)
    {
        builder.ToTable("ai_operations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => AiOperationId.From(value))
            .ValueGeneratedNever();

        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Kind).HasMaxLength(100).IsRequired();
        builder.Property(o => o.InputText).HasColumnType("text").IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(o => o.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));
        builder.Property(o => o.ErrorCode).HasMaxLength(100);
        builder.Property(o => o.ErrorMessage).HasMaxLength(1000);
        builder.Property(o => o.CompletedAtUtc);

        builder.Property(o => o.CreatedAtUtc).IsRequired();
        builder.Property(o => o.UpdatedAtUtc);
        builder.Property(o => o.IsDeleted).IsRequired();
        builder.Property(o => o.DeletedAtUtc);

        builder.MapXminConcurrencyToken(o => o.Version);
        builder.MapTenantScope("ai_operations");

        builder.HasIndex(o => new { o.TenantId, o.UserId }).HasDatabaseName("ix_ai_operations_tenant_user");

        builder.Ignore(o => o.DomainEvents);
    }
}
