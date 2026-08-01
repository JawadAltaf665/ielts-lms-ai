using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "writing_evaluations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    writing_submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_response = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    coherence_cohesion = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    lexical_resource = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    grammatical_range_accuracy = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    estimated_band = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    feedback_text = table.Column<string>(type: "text", nullable: false),
                    requires_human_review = table.Column<bool>(type: "boolean", nullable: false),
                    ai_response_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ai_provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    overridden_task_response = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_coherence_cohesion = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_lexical_resource = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overridden_grammatical_range_accuracy = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
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
                    table.PrimaryKey("pk_writing_evaluations", x => x.id);
                    table.CheckConstraint("ck_writing_evaluations_coherence_cohesion_range", "\"coherence_cohesion\" >= 0 AND \"coherence_cohesion\" <= 9 AND MOD(\"coherence_cohesion\" * 2, 1) = 0");
                    table.CheckConstraint("ck_writing_evaluations_confidence_range", "\"confidence\" >= 0 AND \"confidence\" <= 1");
                    table.CheckConstraint("ck_writing_evaluations_grammatical_range_accuracy_range", "\"grammatical_range_accuracy\" >= 0 AND \"grammatical_range_accuracy\" <= 9 AND MOD(\"grammatical_range_accuracy\" * 2, 1) = 0");
                    table.CheckConstraint("ck_writing_evaluations_lexical_resource_range", "\"lexical_resource\" >= 0 AND \"lexical_resource\" <= 9 AND MOD(\"lexical_resource\" * 2, 1) = 0");
                    table.CheckConstraint("ck_writing_evaluations_task_response_range", "\"task_response\" >= 0 AND \"task_response\" <= 9 AND MOD(\"task_response\" * 2, 1) = 0");
                });

            migrationBuilder.CreateTable(
                name: "writing_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    writing_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    response_text = table.Column<string>(type: "text", nullable: false),
                    word_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    submitted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    writing_evaluation_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("pk_writing_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "writing_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    task_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    track = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    prompt_text = table.Column<string>(type: "text", nullable: false),
                    rubric_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    min_word_count = table.Column<int>(type: "integer", nullable: false),
                    max_word_count = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_writing_tasks", x => x.id);
                    table.CheckConstraint("ck_writing_tasks_word_count_bounds", "\"max_word_count\" >= \"min_word_count\" AND \"min_word_count\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_writing_evaluations_submission_id_unique",
                table: "writing_evaluations",
                column: "writing_submission_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_writing_evaluations_tenant_band",
                table: "writing_evaluations",
                columns: new[] { "tenant_id", "estimated_band" });

            migrationBuilder.CreateIndex(
                name: "ix_writing_evaluations_tenant_id",
                table: "writing_evaluations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_status",
                table: "writing_submissions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_tenant_id",
                table: "writing_submissions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_tenant_user",
                table: "writing_submissions",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_writing_submissions_writing_task_id",
                table: "writing_submissions",
                column: "writing_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_tasks_global_title_unique",
                table: "writing_tasks",
                column: "title",
                unique: true,
                filter: "\"tenant_id\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_writing_tasks_tenant_id",
                table: "writing_tasks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_writing_tasks_tenant_title_unique",
                table: "writing_tasks",
                columns: new[] { "tenant_id", "title" },
                unique: true,
                filter: "\"tenant_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_writing_tasks_track_task_type",
                table: "writing_tasks",
                columns: new[] { "track", "task_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "writing_evaluations");

            migrationBuilder.DropTable(
                name: "writing_submissions");

            migrationBuilder.DropTable(
                name: "writing_tasks");
        }
    }
}
