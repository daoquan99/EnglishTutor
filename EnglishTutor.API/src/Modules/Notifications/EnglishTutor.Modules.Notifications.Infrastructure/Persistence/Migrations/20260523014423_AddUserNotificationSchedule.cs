using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissedStudyReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "MistakeReviewReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "MonthlySummaryEnabled",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PreferredChannel",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "StudyReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "VocabularyReviewReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "WeeklySummaryEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                newName: "QuietHoursEnabled");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "QuietHoursEnd",
                schema: "notifications",
                table: "NotificationSettings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "QuietHoursStart",
                schema: "notifications",
                table: "NotificationSettings",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                schema: "notifications",
                table: "NotificationSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserNotificationSchedules",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PushEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ReminderBeforeMinutes = table.Column<int>(type: "integer", nullable: true),
                    RemindAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    DayOfMonth = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationSchedules_UserId_NotificationType_TargetLan~",
                schema: "notifications",
                table: "UserNotificationSchedules",
                columns: new[] { "UserId", "NotificationType", "TargetLanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotificationSchedules",
                schema: "notifications");

            migrationBuilder.DropColumn(
                name: "QuietHoursEnd",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "QuietHoursStart",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                schema: "notifications",
                table: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "QuietHoursEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                newName: "WeeklySummaryEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "MissedStudyReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MistakeReviewReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MonthlySummaryEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredChannel",
                schema: "notifications",
                table: "NotificationSettings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "StudyReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VocabularyReviewReminderEnabled",
                schema: "notifications",
                table: "NotificationSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
