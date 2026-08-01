using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMockExamModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mock_exam_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mock_exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    listening_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    writing_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    speaking_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    overall_band = table.Column<decimal>(type: "numeric(2,1)", nullable: true),
                    is_finalized = table.Column<bool>(type: "boolean", nullable: false),
                    finalized_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_mock_exam_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monthly_mock_exams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    abandoned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reading_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reading_started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reading_deadline_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reading_completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    listening_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    listening_started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    listening_deadline_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    listening_completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    writing_submission_id = table.Column<Guid>(type: "uuid", nullable: true),
                    writing_started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    writing_deadline_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    writing_completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    speaking_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    speaking_started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    speaking_deadline_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    speaking_completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_monthly_mock_exams", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mock_exam_results_mock_exam_id_unique",
                table: "mock_exam_results",
                column: "mock_exam_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mock_exam_results_tenant_id",
                table: "mock_exam_results",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_listening_attempt_id",
                table: "monthly_mock_exams",
                column: "listening_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_reading_attempt_id",
                table: "monthly_mock_exams",
                column: "reading_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_speaking_attempt_id",
                table: "monthly_mock_exams",
                column: "speaking_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_tenant_id",
                table: "monthly_mock_exams",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_tenant_user_active",
                table: "monthly_mock_exams",
                columns: new[] { "tenant_id", "user_id" },
                unique: true,
                filter: "\"state\" = 'InProgress'");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_mock_exams_writing_submission_id",
                table: "monthly_mock_exams",
                column: "writing_submission_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mock_exam_results");

            migrationBuilder.DropTable(
                name: "monthly_mock_exams");
        }
    }
}
