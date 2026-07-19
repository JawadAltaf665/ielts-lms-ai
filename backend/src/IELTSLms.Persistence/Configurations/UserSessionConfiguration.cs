using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="UserSession"/> aggregate: the device session whose id is surfaced as the
/// <c>sid</c> claim, with sliding and absolute expiry and captured device metadata (Auth §4.1, §4.2).
/// </summary>
internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => UserSessionId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(s => s.DeviceLabel).HasMaxLength(256);
        builder.Property(s => s.IpAddress).HasMaxLength(64);
        builder.Property(s => s.UserAgent).HasMaxLength(512);

        builder.Property(s => s.LastSeenAtUtc).IsRequired();
        builder.Property(s => s.ExpiresAtUtc).IsRequired();
        builder.Property(s => s.AbsoluteExpiresAtUtc).IsRequired();
        builder.Property(s => s.RevokedAtUtc);
        builder.Property(s => s.RevokedReason).HasMaxLength(256);

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);

        builder.MapXminConcurrencyToken(s => s.Version);

        // Listing and revoking a user's active sessions (Auth §4.2).
        builder.HasIndex(s => s.UserId).HasDatabaseName("ix_user_sessions_user_id");

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.IsRevoked);
    }
}
