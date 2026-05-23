using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingTurns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingTurns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingTurns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingTurnResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingTurnResults",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingTurnResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessionSummaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingSessionSummaries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessionSummaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingSessionSummaries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "ConversationPracticeResults",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "ConversationPracticeResults",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "ConversationPracticeResults",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingTurns");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingTurns");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingTurns");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingTurnResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingTurnResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingTurnResults");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessionSummaries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingSessionSummaries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessionSummaries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingSessionSummaries");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "SpeakingSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "speaking",
                table: "ConversationPracticeResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "speaking",
                table: "ConversationPracticeResults");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "speaking",
                table: "ConversationPracticeResults");
        }
    }
}
