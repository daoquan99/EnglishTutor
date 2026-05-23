using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "progress");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "progress",
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
                name: "LearningActivityLogs",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ExpEarned = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpEarned = table.Column<int>(type: "integer", nullable: false),
                    ActivityCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDashboardSnapshots",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalExp = table.Column<int>(type: "integer", nullable: false),
                    CurrentLevel = table.Column<string>(type: "text", nullable: false),
                    StreakDays = table.Column<int>(type: "integer", nullable: false),
                    VocabularyMastered = table.Column<int>(type: "integer", nullable: false),
                    TotalSpeakingSessions = table.Column<int>(type: "integer", nullable: false),
                    TotalExercisesCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalMistakes = table.Column<int>(type: "integer", nullable: false),
                    WeakSkills = table.Column<string>(type: "text", nullable: false),
                    StrongSkills = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDashboardSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserExperiences",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalExp = table.Column<int>(type: "integer", nullable: false),
                    CurrentAppRank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExperiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMonthlyProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    ExpEarned = table.Column<int>(type: "integer", nullable: false),
                    ActivityCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMonthlyProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSkillProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Skill = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ActivityCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkillProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStreaks",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentStreakDays = table.Column<int>(type: "integer", nullable: false),
                    LongestStreakDays = table.Column<int>(type: "integer", nullable: false),
                    LastActivityDateUtc = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStreaks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserWeeklyProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    ExpEarned = table.Column<int>(type: "integer", nullable: false),
                    ActivityCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWeeklyProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperienceTransactions",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperienceTransactions_UserExperiences_UserExperienceId",
                        column: x => x.UserExperienceId,
                        principalSchema: "progress",
                        principalTable: "UserExperiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceTransactions_UserExperienceId",
                schema: "progress",
                table: "ExperienceTransactions",
                column: "UserExperienceId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_HandlerName",
                schema: "progress",
                table: "InboxMessages",
                columns: new[] { "EventId", "HandlerName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningActivityLogs_UserId_TargetLanguageCode_CompletedAtU~",
                schema: "progress",
                table: "LearningActivityLogs",
                columns: new[] { "UserId", "TargetLanguageCode", "CompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyProgresses_UserId_TargetLanguageCode_Date",
                schema: "progress",
                table: "UserDailyProgresses",
                columns: new[] { "UserId", "TargetLanguageCode", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDashboardSnapshots_UserId_TargetLanguageCode_Date",
                schema: "progress",
                table: "UserDashboardSnapshots",
                columns: new[] { "UserId", "TargetLanguageCode", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExperiences_UserId_TargetLanguageCode",
                schema: "progress",
                table: "UserExperiences",
                columns: new[] { "UserId", "TargetLanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMonthlyProgresses_UserId_TargetLanguageCode_Year_Month",
                schema: "progress",
                table: "UserMonthlyProgresses",
                columns: new[] { "UserId", "TargetLanguageCode", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillProgresses_UserId_TargetLanguageCode_Skill",
                schema: "progress",
                table: "UserSkillProgresses",
                columns: new[] { "UserId", "TargetLanguageCode", "Skill" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStreaks_UserId_TargetLanguageCode",
                schema: "progress",
                table: "UserStreaks",
                columns: new[] { "UserId", "TargetLanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyProgresses_UserId_TargetLanguageCode_Year_WeekNum~",
                schema: "progress",
                table: "UserWeeklyProgresses",
                columns: new[] { "UserId", "TargetLanguageCode", "Year", "WeekNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExperienceTransactions",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "LearningActivityLogs",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserDailyProgresses",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserDashboardSnapshots",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserMonthlyProgresses",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserSkillProgresses",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserStreaks",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserWeeklyProgresses",
                schema: "progress");

            migrationBuilder.DropTable(
                name: "UserExperiences",
                schema: "progress");
        }
    }
}
