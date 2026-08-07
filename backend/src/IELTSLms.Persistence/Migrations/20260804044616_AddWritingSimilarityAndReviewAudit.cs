using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingSimilarityAndReviewAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "review_decision",
                table: "writing_evaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reviewed_at_utc",
                table: "writing_evaluations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reviewed_by",
                table: "writing_evaluations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "similarity_score",
                table: "writing_evaluations",
                type: "numeric(3,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_writing_evaluations_tenant_requires_review",
                table: "writing_evaluations",
                columns: new[] { "tenant_id", "requires_human_review", "created_at_utc" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_writing_evaluations_similarity_score_range",
                table: "writing_evaluations",
                sql: "\"similarity_score\" IS NULL OR (\"similarity_score\" >= 0 AND \"similarity_score\" <= 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_writing_evaluations_tenant_requires_review",
                table: "writing_evaluations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_writing_evaluations_similarity_score_range",
                table: "writing_evaluations");

            migrationBuilder.DropColumn(
                name: "review_decision",
                table: "writing_evaluations");

            migrationBuilder.DropColumn(
                name: "reviewed_at_utc",
                table: "writing_evaluations");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                table: "writing_evaluations");

            migrationBuilder.DropColumn(
                name: "similarity_score",
                table: "writing_evaluations");
        }
    }
}
