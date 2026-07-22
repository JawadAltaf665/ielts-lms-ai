using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class KnownDeviceConfiguration : IEntityTypeConfiguration<KnownDevice>
{
    public void Configure(EntityTypeBuilder<KnownDevice> builder)
    {
        builder.ToTable("known_devices");

        builder.HasKey(kd => kd.Id);
        builder.Property(kd => kd.Id)
            .HasConversion(id => id.Value, value => KnownDeviceId.From(value))
            .ValueGeneratedNever();

        builder.Property(kd => kd.TenantId).IsRequired();

        builder.Property(kd => kd.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(kd => kd.DeviceIdentifier).HasMaxLength(256).IsRequired();
        builder.Property(kd => kd.DeviceName).HasMaxLength(256).IsRequired();
        builder.Property(kd => kd.IsTrusted).IsRequired();
        builder.Property(kd => kd.LastSeenAtUtc).IsRequired();
        builder.Property(kd => kd.RemovedAtUtc);

        builder.Property(kd => kd.CreatedAtUtc).IsRequired();
        builder.Property(kd => kd.UpdatedAtUtc);

        builder.MapXminConcurrencyToken(kd => kd.Version);

        builder.HasIndex(kd => new { kd.UserId, kd.DeviceIdentifier })
            .IsUnique()
            .HasDatabaseName("ix_known_devices_user_identifier");

        builder.Ignore(kd => kd.DomainEvents);
        builder.Ignore(kd => kd.IsRemoved);
    }
}
