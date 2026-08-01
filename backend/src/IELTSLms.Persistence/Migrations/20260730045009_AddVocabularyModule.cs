using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IELTSLms.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vocabulary_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_word_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ease_factor = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    interval_days = table.Column<int>(type: "integer", nullable: false),
                    repetition_count = table.Column<int>(type: "integer", nullable: false),
                    next_review_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_grade = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_vocabulary_progress", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_words",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    word = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sense = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    meaning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    examples_json = table.Column<string>(type: "jsonb", nullable: false),
                    frequency_band = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    difficulty_level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
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
                    table.PrimaryKey("pk_vocabulary_words", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_progress_tenant_id",
                table: "vocabulary_progress",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_progress_tenant_user_next_review",
                table: "vocabulary_progress",
                columns: new[] { "tenant_id", "user_id", "next_review_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_progress_tenant_user_word_unique",
                table: "vocabulary_progress",
                columns: new[] { "tenant_id", "user_id", "vocabulary_word_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_words_global_word_sense_unique",
                table: "vocabulary_words",
                columns: new[] { "word", "sense" },
                unique: true,
                filter: "\"tenant_id\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_words_tenant_id",
                table: "vocabulary_words",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_words_tenant_word_sense_unique",
                table: "vocabulary_words",
                columns: new[] { "tenant_id", "word", "sense" },
                unique: true,
                filter: "\"tenant_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_words_topic",
                table: "vocabulary_words",
                column: "topic");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_words_word",
                table: "vocabulary_words",
                column: "word");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vocabulary_progress");

            migrationBuilder.DropTable(
                name: "vocabulary_words");
        }
    }
}
