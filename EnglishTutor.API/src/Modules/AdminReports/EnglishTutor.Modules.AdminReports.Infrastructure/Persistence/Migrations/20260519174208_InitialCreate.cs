using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.AdminReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adminreports");

            migrationBuilder.CreateTable(
                name: "AssessmentPassRateReports",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AssessmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ForLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TotalAttempts = table.Column<int>(type: "integer", nullable: false),
                    PassedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    PassRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentPassRateReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetEntity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetEntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "jsonb", nullable: true),
                    NewValue = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommonMistakeStats",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MistakeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    AffectedUsers = table.Column<int>(type: "integer", nullable: false),
                    ExampleOriginal = table.Column<string>(type: "text", nullable: false),
                    ExampleCorrected = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonMistakeStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyAiUsageReports",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ModelType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaskType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalRequests = table.Column<int>(type: "integer", nullable: false),
                    TotalPromptTokens = table.Column<long>(type: "bigint", nullable: false),
                    TotalCompletionTokens = table.Column<long>(type: "bigint", nullable: false),
                    TotalTokens = table.Column<long>(type: "bigint", nullable: false),
                    AverageLatencyMs = table.Column<int>(type: "integer", nullable: false),
                    FailedRequests = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCostUsd = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAiUsageReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningActivityReports",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalActiveUsers = table.Column<int>(type: "integer", nullable: false),
                    TotalSpeakingSessions = table.Column<int>(type: "integer", nullable: false),
                    TotalExercisesCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalVocabularyReviews = table.Column<int>(type: "integer", nullable: false),
                    TotalLessonsCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalAssessments = table.Column<int>(type: "integer", nullable: false),
                    TotalStudyMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningActivityReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionReports",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActiveUsers = table.Column<int>(type: "integer", nullable: false),
                    ReturningUsers = table.Column<int>(type: "integer", nullable: false),
                    RetentionRate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOverviewCards",
                schema: "adminreports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TotalExp = table.Column<int>(type: "integer", nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "integer", nullable: false),
                    TotalSpeakingSessions = table.Column<int>(type: "integer", nullable: false),
                    TotalExercisesCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalVocabularyMastered = table.Column<int>(type: "integer", nullable: false),
                    TotalMistakes = table.Column<int>(type: "integer", nullable: false),
                    LastActivityAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOverviewCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentPassRateReports_ReportDate_TargetLanguageCode_Ass~",
                schema: "adminreports",
                table: "AssessmentPassRateReports",
                columns: new[] { "ReportDate", "TargetLanguageCode", "AssessmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUserId_CreatedAtUtc",
                schema: "adminreports",
                table: "AuditLogs",
                columns: new[] { "AdminUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetEntity_TargetEntityId",
                schema: "adminreports",
                table: "AuditLogs",
                columns: new[] { "TargetEntity", "TargetEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommonMistakeStats_TargetLanguageCode_OccurrenceCount",
                schema: "adminreports",
                table: "CommonMistakeStats",
                columns: new[] { "TargetLanguageCode", "OccurrenceCount" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyAiUsageReports_ReportDate_ModelType",
                schema: "adminreports",
                table: "DailyAiUsageReports",
                columns: new[] { "ReportDate", "ModelType" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_HandlerName",
                schema: "adminreports",
                table: "InboxMessages",
                columns: new[] { "EventId", "HandlerName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningActivityReports_ReportDate_Period",
                schema: "adminreports",
                table: "LearningActivityReports",
                columns: new[] { "ReportDate", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_UserOverviewCards_LastActivityAtUtc",
                schema: "adminreports",
                table: "UserOverviewCards",
                column: "LastActivityAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserOverviewCards_RegisteredAtUtc",
                schema: "adminreports",
                table: "UserOverviewCards",
                column: "RegisteredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserOverviewCards_TargetLanguageCode",
                schema: "adminreports",
                table: "UserOverviewCards",
                column: "TargetLanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserOverviewCards_TotalExp",
                schema: "adminreports",
                table: "UserOverviewCards",
                column: "TotalExp");

            migrationBuilder.CreateIndex(
                name: "IX_UserOverviewCards_UserId",
                schema: "adminreports",
                table: "UserOverviewCards",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentPassRateReports",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "CommonMistakeStats",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "DailyAiUsageReports",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "LearningActivityReports",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "RetentionReports",
                schema: "adminreports");

            migrationBuilder.DropTable(
                name: "UserOverviewCards",
                schema: "adminreports");
        }
    }
}
