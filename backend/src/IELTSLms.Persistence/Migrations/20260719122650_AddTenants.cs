using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "user_sessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "refresh_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "password_reset_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "login_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "email_verification_tokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    primary_domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    default_locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    default_time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_tenant_id",
                table: "user_sessions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_tenant_id",
                table: "refresh_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_tenant_id",
                table: "password_reset_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_login_history_tenant_id",
                table: "login_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_tokens_tenant_id",
                table: "email_verification_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_code",
                table: "tenants",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_slug",
                table: "tenants",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_status",
                table: "tenants",
                column: "status");

            // FK constraints to tenants are added here rather than modelled in EF: the dependent
            // columns are plain Guids while Tenant's PK is a strongly-typed TenantId, which EF will not
            // relate across incompatible CLR types (see ConfigurationExtensions.MapTenantScope). RESTRICT
            // so a tenant with live rows cannot be hard-deleted — closure is a soft delete.
            migrationBuilder.AddForeignKey(
                name: "fk_users_tenants_tenant_id",
                table: "users",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_sessions_tenants_tenant_id",
                table: "user_sessions",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_tenants_tenant_id",
                table: "refresh_tokens",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_password_reset_tokens_tenants_tenant_id",
                table: "password_reset_tokens",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_login_history_tenants_tenant_id",
                table: "login_history",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_email_verification_tokens_tenants_tenant_id",
                table: "email_verification_tokens",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_tenants_tenant_id",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "fk_user_sessions_tenants_tenant_id",
                table: "user_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_tenants_tenant_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_password_reset_tokens_tenants_tenant_id",
                table: "password_reset_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_login_history_tenants_tenant_id",
                table: "login_history");

            migrationBuilder.DropForeignKey(
                name: "fk_email_verification_tokens_tenants_tenant_id",
                table: "email_verification_tokens");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_user_sessions_tenant_id",
                table: "user_sessions");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_tenant_id",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "ix_password_reset_tokens_tenant_id",
                table: "password_reset_tokens");

            migrationBuilder.DropIndex(
                name: "ix_login_history_tenant_id",
                table: "login_history");

            migrationBuilder.DropIndex(
                name: "ix_email_verification_tokens_tenant_id",
                table: "email_verification_tokens");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "password_reset_tokens");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "login_history");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "email_verification_tokens");
        }
    }
}
