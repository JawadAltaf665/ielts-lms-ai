using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("organization_members");

        builder.HasKey(om => om.Id);
        builder.Property(om => om.Id).ValueGeneratedNever();

        builder.Property(om => om.TenantId).IsRequired();

        builder.Property(om => om.OrganizationId)
            .HasConversion(id => id.Value, value => OrganizationId.From(value))
            .IsRequired();

        builder.Property(om => om.UserId).IsRequired();

        builder.Property(om => om.CreatedAtUtc).IsRequired();
        builder.Property(om => om.UpdatedAtUtc);

        builder.HasIndex(om => new { om.OrganizationId, om.UserId })
            .IsUnique()
            .HasDatabaseName("ix_organization_members_organization_user");
    }
}
