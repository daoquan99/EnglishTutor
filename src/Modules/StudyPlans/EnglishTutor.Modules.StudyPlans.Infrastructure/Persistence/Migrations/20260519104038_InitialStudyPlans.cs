using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStudyPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "studyplans");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "studyplans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<string>(type: "text", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedStudySessions",
                schema: "studyplans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ScheduledDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MissedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PlannedStudySessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStudyPlans",
                schema: "studyplans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLanguageCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PreferredStudyTimeUtc = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ReminderBeforeMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DailyTargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    WeeklyTargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    MonthlyTargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    MonthlyTargetStudyDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_UserStudyPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlanTargets",
                schema: "studyplans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetMinutes = table.Column<int>(type: "integer", nullable: false),
                    TargetStudyDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlanTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPlanTargets_UserStudyPlans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalSchema: "studyplans",
                        principalTable: "UserStudyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStudyWeekDays",
                schema: "studyplans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsStudyDay = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStudyWeekDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStudyWeekDays_UserStudyPlans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalSchema: "studyplans",
                        principalTable: "UserStudyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextRetryAtUtc",
                schema: "studyplans",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedStudySessions_Status_ScheduledDateUtc",
                schema: "studyplans",
                table: "PlannedStudySessions",
                columns: new[] { "Status", "ScheduledDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedStudySessions_StudyPlanId_ScheduledDateUtc",
                schema: "studyplans",
                table: "PlannedStudySessions",
                columns: new[] { "StudyPlanId", "ScheduledDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedStudySessions_UserId_ScheduledDateUtc_Status",
                schema: "studyplans",
                table: "PlannedStudySessions",
                columns: new[] { "UserId", "ScheduledDateUtc", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanTargets_StudyPlanId_Period",
                schema: "studyplans",
                table: "StudyPlanTargets",
                columns: new[] { "StudyPlanId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStudyPlans_UserId_TargetLanguageCode",
                schema: "studyplans",
                table: "UserStudyPlans",
                columns: new[] { "UserId", "TargetLanguageCode" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_UserStudyWeekDays_StudyPlanId_DayOfWeek",
                schema: "studyplans",
                table: "UserStudyWeekDays",
                columns: new[] { "StudyPlanId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "studyplans");

            migrationBuilder.DropTable(
                name: "PlannedStudySessions",
                schema: "studyplans");

            migrationBuilder.DropTable(
                name: "StudyPlanTargets",
                schema: "studyplans");

            migrationBuilder.DropTable(
                name: "UserStudyWeekDays",
                schema: "studyplans");

            migrationBuilder.DropTable(
                name: "UserStudyPlans",
                schema: "studyplans");
        }
    }
}
