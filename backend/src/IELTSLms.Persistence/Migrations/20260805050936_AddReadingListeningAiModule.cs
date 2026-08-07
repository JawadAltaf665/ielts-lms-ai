using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingListeningAiModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "question_variant_drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    source_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_content = table.Column<string>(type: "jsonb", nullable: false),
                    difficulty_delta = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ai_response_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    restored_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    restored_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_variant_drafts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_question_variant_drafts_source",
                table: "question_variant_drafts",
                columns: new[] { "source_type", "source_question_id" });

            migrationBuilder.CreateIndex(
                name: "ix_question_variant_drafts_tenant_id",
                table: "question_variant_drafts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_variant_drafts_tenant_status",
                table: "question_variant_drafts",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_variant_drafts");
        }
    }
}
