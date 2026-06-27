using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Feedback.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFeedbackSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "feedback");

            migrationBuilder.CreateTable(
                name: "session_feedbacks",
                schema: "feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    practice_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    strengths = table.Column<string>(type: "text", nullable: false),
                    improvement_areas = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: true),
                    cefr_level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    failure_reason_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_feedbacks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "corrections",
                schema: "feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_text = table.Column<string>(type: "text", nullable: false),
                    corrected_text = table.Column<string>(type: "text", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corrections", x => x.id);
                    table.ForeignKey(
                        name: "FK_corrections_session_feedbacks_session_feedback_id",
                        column: x => x.session_feedback_id,
                        principalSchema: "feedback",
                        principalTable: "session_feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extracted_vocabulary",
                schema: "feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    term = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    meaning = table.Column<string>(type: "text", nullable: false),
                    example_sentence = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracted_vocabulary", x => x.id);
                    table.ForeignKey(
                        name: "FK_extracted_vocabulary_session_feedbacks_session_feedback_id",
                        column: x => x.session_feedback_id,
                        principalSchema: "feedback",
                        principalTable: "session_feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mistake_patterns",
                schema: "feedback",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pattern = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mistake_patterns", x => x.id);
                    table.ForeignKey(
                        name: "FK_mistake_patterns_session_feedbacks_session_feedback_id",
                        column: x => x.session_feedback_id,
                        principalSchema: "feedback",
                        principalTable: "session_feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_corrections_session_feedback_id",
                schema: "feedback",
                table: "corrections",
                column: "session_feedback_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_vocabulary_session_feedback_id",
                schema: "feedback",
                table: "extracted_vocabulary",
                column: "session_feedback_id");

            migrationBuilder.CreateIndex(
                name: "IX_mistake_patterns_session_feedback_id",
                schema: "feedback",
                table: "mistake_patterns",
                column: "session_feedback_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_feedbacks_practice_session_id",
                schema: "feedback",
                table: "session_feedbacks",
                column: "practice_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_feedbacks_status",
                schema: "feedback",
                table: "session_feedbacks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_session_feedbacks_user_id",
                schema: "feedback",
                table: "session_feedbacks",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corrections",
                schema: "feedback");

            migrationBuilder.DropTable(
                name: "extracted_vocabulary",
                schema: "feedback");

            migrationBuilder.DropTable(
                name: "mistake_patterns",
                schema: "feedback");

            migrationBuilder.DropTable(
                name: "session_feedbacks",
                schema: "feedback");
        }
    }
}
