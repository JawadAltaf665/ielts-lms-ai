using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeakingReviewAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "review_decision",
                table: "speaking_evaluations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reviewed_at_utc",
                table: "speaking_evaluations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reviewed_by",
                table: "speaking_evaluations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_speaking_evaluations_tenant_requires_review",
                table: "speaking_evaluations",
                columns: new[] { "tenant_id", "requires_human_review", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_speaking_evaluations_tenant_requires_review",
                table: "speaking_evaluations");

            migrationBuilder.DropColumn(
                name: "review_decision",
                table: "speaking_evaluations");

            migrationBuilder.DropColumn(
                name: "reviewed_at_utc",
                table: "speaking_evaluations");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                table: "speaking_evaluations");
        }
    }
}
