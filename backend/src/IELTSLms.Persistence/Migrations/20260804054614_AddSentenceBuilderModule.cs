using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSentenceBuilderModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sentence_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentence_exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: true),
                    ai_response_id = table.Column<Guid>(type: "uuid", nullable: true),
                    feedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("pk_sentence_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sentence_exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    target_pattern_json = table.Column<string>(type: "jsonb", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    restored_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    restored_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sentence_exercises", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sentence_attempts_exercise_id",
                table: "sentence_attempts",
                column: "sentence_exercise_id");

            migrationBuilder.CreateIndex(
                name: "ix_sentence_attempts_tenant_id",
                table: "sentence_attempts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sentence_attempts_tenant_user",
                table: "sentence_attempts",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sentence_exercises_difficulty",
                table: "sentence_exercises",
                column: "difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_sentence_exercises_tenant_id",
                table: "sentence_exercises",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sentence_attempts");

            migrationBuilder.DropTable(
                name: "sentence_exercises");
        }
    }
}
