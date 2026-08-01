using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeakingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "speaking_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    speaking_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    transcript_text = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    speaking_evaluation_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_speaking_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "speaking_evaluations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    speaking_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fluency_coherence = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    lexical_resource = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    grammatical_range_accuracy = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    pronunciation = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    estimated_band = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    feedback_text = table.Column<string>(type: "text", nullable: false),
                    requires_human_review = table.Column<bool>(type: "boolean", nullable: false),
                    ai_response_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ai_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    overridden_fluency_coherence = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_lexical_resource = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_grammatical_range_accuracy = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_pronunciation = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_by = table.Column<Guid>(type: "uuid", nullable: true),
                    overridden_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    override_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_speaking_evaluations", x => x.id);
                    table.CheckConstraint("ck_speaking_evaluations_confidence_range", "\"confidence\" >= 0 AND \"confidence\" <= 1");
                    table.CheckConstraint("ck_speaking_evaluations_fluency_coherence_range", "\"fluency_coherence\" >= 0 AND \"fluency_coherence\" <= 9 AND MOD(\"fluency_coherence\" * 2, 1) = 0");
                    table.CheckConstraint("ck_speaking_evaluations_grammatical_range_accuracy_range", "\"grammatical_range_accuracy\" >= 0 AND \"grammatical_range_accuracy\" <= 9 AND MOD(\"grammatical_range_accuracy\" * 2, 1) = 0");
                    table.CheckConstraint("ck_speaking_evaluations_lexical_resource_range", "\"lexical_resource\" >= 0 AND \"lexical_resource\" <= 9 AND MOD(\"lexical_resource\" * 2, 1) = 0");
                    table.CheckConstraint("ck_speaking_evaluations_pronunciation_range", "\"pronunciation\" >= 0 AND \"pronunciation\" <= 9 AND MOD(\"pronunciation\" * 2, 1) = 0");
                });

            migrationBuilder.CreateTable(
                name: "speaking_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    part = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    prompt_text = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_speaking_tasks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_speaking_attempts_speaking_task_id",
                table: "speaking_attempts",
                column: "speaking_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_attempts_status",
                table: "speaking_attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_attempts_tenant_id",
                table: "speaking_attempts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_attempts_tenant_user",
                table: "speaking_attempts",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_speaking_evaluations_attempt_id_unique",
                table: "speaking_evaluations",
                column: "speaking_attempt_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_speaking_evaluations_tenant_band",
                table: "speaking_evaluations",
                columns: new[] { "tenant_id", "estimated_band" });

            migrationBuilder.CreateIndex(
                name: "ix_speaking_evaluations_tenant_id",
                table: "speaking_evaluations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_tasks_global_title_unique",
                table: "speaking_tasks",
                column: "title",
                unique: true,
                filter: "\"tenant_id\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_tasks_part",
                table: "speaking_tasks",
                column: "part");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_tasks_tenant_id",
                table: "speaking_tasks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_speaking_tasks_tenant_title_unique",
                table: "speaking_tasks",
                columns: new[] { "tenant_id", "title" },
                unique: true,
                filter: "\"tenant_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "speaking_attempts");

            migrationBuilder.DropTable(
                name: "speaking_evaluations");

            migrationBuilder.DropTable(
                name: "speaking_tasks");
        }
    }
}
