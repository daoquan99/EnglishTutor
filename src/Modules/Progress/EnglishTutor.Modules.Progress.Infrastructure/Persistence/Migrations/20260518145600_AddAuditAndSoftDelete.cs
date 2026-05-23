using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserWeeklyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserWeeklyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserWeeklyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserWeeklyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserStreaks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserStreaks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserStreaks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserStreaks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserSkillProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserSkillProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserSkillProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserMonthlyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserMonthlyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserMonthlyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserMonthlyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserExperiences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserExperiences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                schema: "progress",
                table: "UserExperiences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "progress",
                table: "UserExperiences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "progress",
                table: "UserExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserExperiences",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserDashboardSnapshots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserDashboardSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserDashboardSnapshots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserDashboardSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserDailyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserDailyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserDailyProgresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserDailyProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "LearningActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "LearningActivityLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "LearningActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "LearningActivityLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "progress",
                table: "ExperienceTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "ExperienceTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "ExperienceTransactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserWeeklyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserWeeklyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserWeeklyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserWeeklyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserStreaks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserStreaks");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserStreaks");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserStreaks");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserSkillProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserSkillProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserSkillProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserMonthlyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserMonthlyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserMonthlyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserMonthlyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserExperiences");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserDashboardSnapshots");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserDashboardSnapshots");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserDashboardSnapshots");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserDashboardSnapshots");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "UserDailyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "UserDailyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "UserDailyProgresses");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "UserDailyProgresses");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "progress",
                table: "LearningActivityLogs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "LearningActivityLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "LearningActivityLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "LearningActivityLogs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "progress",
                table: "ExperienceTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "progress",
                table: "ExperienceTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                schema: "progress",
                table: "ExperienceTransactions");
        }
    }
}
