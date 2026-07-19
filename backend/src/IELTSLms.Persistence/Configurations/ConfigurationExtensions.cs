using System;
using System.Linq.Expressions;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Shared mapping helpers so every identity entity treats optimistic concurrency and tenant scoping
/// identically (Backend Architecture §9, Multi-Tenant Architecture §6): the domain's <c>uint Version</c>
/// is bound to PostgreSQL's <c>xmin</c> system column, and every <see cref="ITenantScoped"/> entity gets
/// the same required <c>tenant_id</c> column, foreign key, and index.
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Maps a <c>uint Version</c> property onto the PostgreSQL <c>xmin</c> system column as the
    /// concurrency token. No physical column is added; <c>xmin</c> already exists on every row.
    /// </summary>
    public static void MapXminConcurrencyToken<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, uint>> versionProperty)
        where TEntity : class
    {
        builder.Property(versionProperty)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }

    /// <summary>
    /// Maps the required <c>TenantId</c> discriminator common to every tenant-scoped entity (ERD §3):
    /// the required column and a non-unique index for tenant-filter locality. The global tenant query
    /// filter is applied centrally in <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <remarks>
    /// The foreign-key <em>constraint</em> to <c>tenants</c> is declared in the migration rather than
    /// modelled here: the dependent column is a <see cref="Guid"/> (the <c>ITenantScoped.TenantId</c>
    /// contract lives in SharedKernel and cannot reference the Domain's strongly-typed <c>TenantId</c>),
    /// while the <c>Tenant</c> primary key is the strongly-typed <c>TenantId</c>, and EF Core will not
    /// model a relationship across those incompatible CLR types. The constraint is a plain
    /// <c>RESTRICT</c> FK at the database level (a tenant with live rows cannot be hard-deleted —
    /// closure is a soft delete); EF leaves that untracked constraint untouched across migrations.
    /// </remarks>
    /// <param name="tableName">The entity's table, used to name the index consistently.</param>
    public static void MapTenantScope<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string tableName)
        where TEntity : class, ITenantScoped
    {
        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId).HasDatabaseName($"ix_{tableName}_tenant_id");
    }
}
