using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrganizationId.From(value))
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId).IsRequired();

        builder.Property(o => o.Name).HasMaxLength(256).IsRequired();
        
        builder.Property(o => o.ParentOrganizationId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? OrganizationId.From(value.Value) : (OrganizationId?)null);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(o => o.CreatedAtUtc).IsRequired();
        builder.Property(o => o.UpdatedAtUtc);
        
        builder.Property(o => o.IsDeleted).IsRequired();
        builder.Property(o => o.DeletedAtUtc);

        builder.MapXminConcurrencyToken(o => o.Version);

        builder.Ignore(o => o.DomainEvents);
    }
}
