using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="User"/> aggregate: strongly-typed id and value-object conversions, the
/// <c>(tenant_id, email)</c> uniqueness rule (Sprint 2 scope — email is unique per tenant), audit
/// and soft-delete columns, and the PostgreSQL <c>xmin</c> optimistic-concurrency token.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .ValueGeneratedNever();

        builder.Property(u => u.TenantId).IsRequired();

        // Email value object -> single normalized column; unique per tenant (Sprint 2 resolution).
        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.FromTrusted(value))
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasConversion(hash => hash.Value, value => PasswordHash.FromHashed(value))
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(u => u.EmailVerifiedAtUtc);
        builder.Property(u => u.FailedLoginAttempts).IsRequired();
        builder.Property(u => u.LockoutEndsAtUtc);
        builder.Property(u => u.LastLoginAtUtc);

        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.UpdatedAtUtc);
        builder.Property(u => u.IsDeleted).IsRequired();
        builder.Property(u => u.DeletedAtUtc);

        // Map the domain's Version onto the PostgreSQL system column xmin as the concurrency token
        // (Backend Architecture §9), so optimistic concurrency needs no extra stored column.
        builder.MapXminConcurrencyToken(u => u.Version);

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasDatabaseName("ix_users_tenant_id_email");

        // Soft-deleted accounts are filtered out of normal queries (Development Standards §15).
        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Ignore(u => u.DomainEvents);
        builder.Ignore(u => u.IsEmailVerified);
    }
}
