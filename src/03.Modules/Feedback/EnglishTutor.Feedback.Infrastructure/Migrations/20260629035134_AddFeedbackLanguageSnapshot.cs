using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Feedback.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackLanguageSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "explanation_language_code",
                schema: "feedback",
                table: "session_feedbacks",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<Guid>(
                name: "language_pair_id",
                schema: "feedback",
                table: "session_feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<string>(
                name: "native_language_code",
                schema: "feedback",
                table: "session_feedbacks",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<string>(
                name: "target_language_code",
                schema: "feedback",
                table: "session_feedbacks",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.CreateIndex(
                name: "ix_session_feedbacks_user_language_pair_created",
                schema: "feedback",
                table: "session_feedbacks",
                columns: new[] { "user_id", "language_pair_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_session_feedbacks_user_language_pair_created",
                schema: "feedback",
                table: "session_feedbacks");

            migrationBuilder.DropColumn(
                name: "explanation_language_code",
                schema: "feedback",
                table: "session_feedbacks");

            migrationBuilder.DropColumn(
                name: "language_pair_id",
                schema: "feedback",
                table: "session_feedbacks");

            migrationBuilder.DropColumn(
                name: "native_language_code",
                schema: "feedback",
                table: "session_feedbacks");

            migrationBuilder.DropColumn(
                name: "target_language_code",
                schema: "feedback",
                table: "session_feedbacks");
        }
    }
}
